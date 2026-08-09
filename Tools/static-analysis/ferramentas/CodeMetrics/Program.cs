// Analise estatica de codigo C# baseada em Roslyn (syntax-only, nao requer compilacao).
// Metricas: LOC, McCabe Cyclomatic Complexity, Halstead Volume, Maintainability Index (Microsoft),
// profundidade de aninhamento, acoplamento eferente, membros publicos, deteccao de God Class / Long Method.
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length < 3) { Console.Error.WriteLine("uso: <label> <raiz> <saidaCsvPrefixo> [pastasExcluidas...]"); return 1; }
string label = args[0], root = args[1], outPrefix = args[2];
var excl = args.Skip(3).ToArray();

var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
    .Where(f => !excl.Any(e => f.Replace('/', '\\').Contains("\\" + e + "\\", StringComparison.OrdinalIgnoreCase)))
    .OrderBy(f => f).ToList();

var methods = new List<MethodRow>();
var types = new List<TypeRow>();
int totalPhysical = 0, totalSource = 0, totalComment = 0;
int nInterfaces = 0, nClasses = 0, nStructs = 0, nEnums = 0, nMonoBehaviour = 0;

foreach (var file in files)
{
    var text = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(text);
    var rootNode = tree.GetRoot();
    var rel = Path.GetRelativePath(root, file);

    var lines = text.Replace("\r\n", "\n").Split('\n');
    totalPhysical += lines.Length;
    foreach (var l in lines)
    {
        var t = l.Trim();
        if (t.Length == 0) continue;
        if (t.StartsWith("//") || t.StartsWith("///") || t.StartsWith("*") || t.StartsWith("/*")) totalComment++;
        else totalSource++;
    }

    foreach (var td in rootNode.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
    {
        string kind = td switch
        {
            InterfaceDeclarationSyntax => "interface",
            ClassDeclarationSyntax => "class",
            StructDeclarationSyntax => "struct",
            RecordDeclarationSyntax => "record",
            EnumDeclarationSyntax => "enum",
            _ => "other"
        };
        if (kind == "interface") nInterfaces++;
        else if (kind == "class") nClasses++;
        else if (kind == "struct" || kind == "record") nStructs++;
        else if (kind == "enum") nEnums++;

        var bases = td.BaseList?.Types.Select(b => b.Type.ToString()) ?? Enumerable.Empty<string>();
        bool isMono = bases.Any(b => b.Contains("MonoBehaviour") || b.Contains("ScriptableObject"));
        if (isMono) nMonoBehaviour++;

        var memberNodes = (td as TypeDeclarationSyntax)?.Members ?? default;
        int publicMembers = 0, fieldCount = 0, methodCount = 0;
        if (memberNodes != default)
            foreach (var m in memberNodes)
            {
                var mods = m.Modifiers.Select(x => x.Text).ToList();
                if (mods.Contains("public")) publicMembers++;
                if (m is FieldDeclarationSyntax fd) fieldCount += fd.Declaration.Variables.Count;
                if (m is MethodDeclarationSyntax) methodCount++;
            }

        // acoplamento eferente aproximado: tipos distintos referenciados no corpo
        var referenced = td.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Select(i => i.Identifier.Text)
            .Where(n => n.Length > 1 && char.IsUpper(n[0]))
            .Distinct().Count();

        var span = td.GetLocation().GetLineSpan();
        int typeLoc = span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
        int typeCc = Cc(td);

        types.Add(new TypeRow(label, rel, td.Identifier.Text, kind, typeLoc, typeCc,
            publicMembers, fieldCount, methodCount, referenced, isMono,
            string.Join(" ", bases)));
    }

    foreach (var node in rootNode.DescendantNodes())
    {
        string? name = null; SyntaxNode? body = null; int paramCount = 0;
        switch (node)
        {
            case MethodDeclarationSyntax m:
                name = m.Identifier.Text; body = (SyntaxNode?)m.Body ?? m.ExpressionBody; paramCount = m.ParameterList.Parameters.Count; break;
            case ConstructorDeclarationSyntax c:
                name = c.Identifier.Text + " (ctor)"; body = (SyntaxNode?)c.Body ?? c.ExpressionBody; paramCount = c.ParameterList.Parameters.Count; break;
            case PropertyDeclarationSyntax p when p.AccessorList != null:
                name = p.Identifier.Text + " (prop)"; body = p.AccessorList; break;
            default: continue;
        }
        if (body == null || name == null) continue;

        var owner = node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault()?.Identifier.Text ?? "-";
        var sp = node.GetLocation().GetLineSpan();
        int loc = sp.EndLinePosition.Line - sp.StartLinePosition.Line + 1;
        int cc = Cc(body);
        int depth = MaxNesting(body);
        double vol = Halstead(body);
        double mi = MaintainabilityIndex(vol, cc, Math.Max(loc, 1));
        methods.Add(new MethodRow(label, rel, owner, name, loc, cc, depth, paramCount, Math.Round(vol, 1), Math.Round(mi, 1)));
    }
}

// ---------- saida ----------
var ci = CultureInfo.InvariantCulture;
var sbM = new StringBuilder("projeto;arquivo;tipo;membro;loc;cc;aninhamento;parametros;halstead_volume;maintainability_index\n");
foreach (var m in methods)
    sbM.Append($"{m.Label};{m.File};{m.Owner};{m.Name};{m.Loc};{m.Cc};{m.Depth};{m.Params};{m.Vol.ToString(ci)};{m.Mi.ToString(ci)}\n");
File.WriteAllText(outPrefix + "-metodos.csv", sbM.ToString(), Encoding.UTF8);

var sbT = new StringBuilder("projeto;arquivo;tipo;especie;loc;cc;membros_publicos;campos;metodos;acoplamento_eferente;unity;heranca\n");
foreach (var t in types)
    sbT.Append($"{t.Label};{t.File};{t.Name};{t.Kind};{t.Loc};{t.Cc};{t.PublicMembers};{t.Fields};{t.Methods};{t.Coupling};{(t.IsUnity ? 1 : 0)};{t.Bases}\n");
File.WriteAllText(outPrefix + "-tipos.csv", sbT.ToString(), Encoding.UTF8);

// ---------- resumo ----------
double MedianOf(IEnumerable<double> xs) { var a = xs.OrderBy(x => x).ToArray(); return a.Length == 0 ? 0 : (a.Length % 2 == 1 ? a[a.Length / 2] : (a[a.Length / 2 - 1] + a[a.Length / 2]) / 2.0); }
var ccs = methods.Select(m => (double)m.Cc).ToList();
var mis = methods.Select(m => m.Mi).ToList();
var locs = methods.Select(m => (double)m.Loc).ToList();

int ccHigh = methods.Count(m => m.Cc > 10);
int ccVeryHigh = methods.Count(m => m.Cc > 20);
int longMethods = methods.Count(m => m.Loc > 60);
int deepNest = methods.Count(m => m.Depth >= 4);
int miLow = methods.Count(m => m.Mi < 20);
int miMed = methods.Count(m => m.Mi >= 20 && m.Mi < 65);
var godClasses = types.Where(t => t.Kind == "class" && (t.Loc > 300 || t.PublicMembers > 25)).ToList();

var r = new StringBuilder();
void W(string s) { r.AppendLine(s); Console.WriteLine(s); }
W($"===== {label} =====");
W($"arquivos analisados            : {files.Count}");
W($"linhas fisicas                 : {totalPhysical}");
W($"linhas de codigo (sem branco)  : {totalSource}");
W($"linhas de comentario           : {totalComment}");
W($"densidade de comentarios (%)   : {(totalSource + totalComment == 0 ? 0 : 100.0 * totalComment / (totalSource + totalComment)):F1}");
W($"classes                        : {nClasses}");
W($"interfaces                     : {nInterfaces}");
W($"structs/records                : {nStructs}");
W($"enums                          : {nEnums}");
W($"tipos acoplados a Unity        : {nMonoBehaviour}  ({(nClasses == 0 ? 0 : 100.0 * nMonoBehaviour / nClasses):F1}% das classes)");
W($"membros analisados             : {methods.Count}");
W("");
W($"CC media                       : {(ccs.Count == 0 ? 0 : ccs.Average()):F2}");
W($"CC mediana                     : {MedianOf(ccs):F1}");
W($"CC maxima                      : {(ccs.Count == 0 ? 0 : ccs.Max()):F0}");
W($"CC total (soma)                : {ccs.Sum():F0}");
W($"membros com CC > 10            : {ccHigh}  ({(methods.Count == 0 ? 0 : 100.0 * ccHigh / methods.Count):F1}%)");
W($"membros com CC > 20            : {ccVeryHigh}");
W("");
W($"Maintainability Index medio    : {(mis.Count == 0 ? 0 : mis.Average()):F1}");
W($"MI mediano                     : {MedianOf(mis):F1}");
W($"membros com MI < 20 (ruim)     : {miLow}");
W($"membros com MI 20-64 (moderado): {miMed}");
W($"membros com MI >= 65 (bom)     : {methods.Count - miLow - miMed}  ({(methods.Count == 0 ? 0 : 100.0 * (methods.Count - miLow - miMed) / methods.Count):F1}%)");
W("");
W($"LOC media por membro           : {(locs.Count == 0 ? 0 : locs.Average()):F1}");
W($"membros com mais de 60 linhas  : {longMethods}");
W($"membros com aninhamento >= 4   : {deepNest}");
W($"acoplamento eferente medio     : {(types.Count == 0 ? 0 : types.Average(t => (double)t.Coupling)):F1}");
W("");
W($"God Classes (>300 LOC ou >25 membros publicos) : {godClasses.Count}");
foreach (var g in godClasses.OrderByDescending(g => g.Loc).Take(10))
    W($"   {g.Name,-34} {g.Loc,4} LOC  {g.PublicMembers,3} pub  CC {g.Cc,4}  {g.File}");
W("");
W("TOP 10 membros por complexidade ciclomatica:");
foreach (var m in methods.OrderByDescending(m => m.Cc).Take(10))
    W($"   CC {m.Cc,3}  MI {m.Mi,5:F1}  {m.Loc,4} LOC  {m.Owner}.{m.Name}");

File.WriteAllText(outPrefix + "-resumo.txt", r.ToString(), Encoding.UTF8);

return 0;

// ---------- funcoes de metrica ----------
static int Cc(SyntaxNode n)
{
    int c = 1;
    foreach (var d in n.DescendantNodes())
    {
        switch (d)
        {
            case IfStatementSyntax:
            case WhileStatementSyntax:
            case ForStatementSyntax:
            case ForEachStatementSyntax:
            case DoStatementSyntax:
            case CatchClauseSyntax:
            case ConditionalExpressionSyntax:
            case CaseSwitchLabelSyntax:
            case CasePatternSwitchLabelSyntax:
            case SwitchExpressionArmSyntax:
                c++; break;
            case BinaryExpressionSyntax b
                when b.IsKind(SyntaxKind.LogicalAndExpression) || b.IsKind(SyntaxKind.LogicalOrExpression)
                  || b.IsKind(SyntaxKind.CoalesceExpression):
                c++; break;
            case ConditionalAccessExpressionSyntax:
                c++; break;
        }
    }
    return c;
}

static int MaxNesting(SyntaxNode n)
{
    int max = 0;
    void Walk(SyntaxNode node, int d)
    {
        foreach (var ch in node.ChildNodes())
        {
            int nd = ch is BlockSyntax || ch is IfStatementSyntax || ch is ForStatementSyntax
                  || ch is ForEachStatementSyntax || ch is WhileStatementSyntax || ch is SwitchStatementSyntax
                  || ch is TryStatementSyntax ? d + 1 : d;
            if (nd > max) max = nd;
            Walk(ch, nd);
        }
    }
    Walk(n, 0);
    return max;
}

static double Halstead(SyntaxNode n)
{
    var operators = new HashSet<string>();
    var operands = new HashSet<string>();
    int N1 = 0, N2 = 0;
    foreach (var tk in n.DescendantTokens())
    {
        var k = tk.Kind();
        bool isOperand = k == SyntaxKind.IdentifierToken || k == SyntaxKind.NumericLiteralToken
                       || k == SyntaxKind.StringLiteralToken || k == SyntaxKind.CharacterLiteralToken
                       || k == SyntaxKind.TrueKeyword || k == SyntaxKind.FalseKeyword || k == SyntaxKind.NullKeyword;
        if (isOperand) { operands.Add(tk.Text); N2++; }
        else if (SyntaxFacts.IsPunctuation(k) || SyntaxFacts.IsKeywordKind(k)) { operators.Add(tk.Text); N1++; }
    }
    int n1 = operators.Count, n2 = operands.Count;
    int vocab = n1 + n2;
    if (vocab <= 1) return 0;
    return (N1 + N2) * Math.Log2(vocab);
}

// Maintainability Index, variante da Microsoft (escala 0-100)
static double MaintainabilityIndex(double volume, int cc, int loc)
{
    double v = volume <= 0 ? 1 : volume;
    double raw = 171 - 5.2 * Math.Log(v) - 0.23 * cc - 16.2 * Math.Log(loc);
    return Math.Max(0, raw * 100.0 / 171.0);
}

record MethodRow(string Label, string File, string Owner, string Name, int Loc, int Cc, int Depth, int Params, double Vol, double Mi);
record TypeRow(string Label, string File, string Name, string Kind, int Loc, int Cc, int PublicMembers, int Fields, int Methods, int Coupling, bool IsUnity, string Bases);
