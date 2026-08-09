// Gera um .csproj de analise que compila apenas Assets/Scripts de um alvo,
// reaproveitando as referencias do Assembly-CSharp.csproj gerado pela Unity.
const fs = require('fs');
const pathMod = require('path');

const [, , templateCsproj, targetScripts, outCsproj, sarifOut] = process.argv;
const tpl = fs.readFileSync(templateCsproj, 'utf8');

const tplDirEarly = pathMod.dirname(pathMod.resolve(templateCsproj));
// HintPath relativo quebra fora da raiz do projeto Unity: converte para absoluto
let refs = (tpl.match(/<Reference Include=[\s\S]*?<\/Reference>/g) || [])
  .map(r => r.replace(/<HintPath>([^<]*)<\/HintPath>/g, (m, p) =>
    '<HintPath>' + (pathMod.isAbsolute(p) ? p : pathMod.join(tplDirEarly, p)) + '</HintPath>'))
  .join('\n    ');

// DLLs de pacote que o Unity resolve via ProjectReference e nao aparecem como <Reference>
const extraDlls = ['Unity.TextMeshPro.dll', 'UnityEngine.UI.dll', 'Unity.InputSystem.dll'];
for (const dll of extraDlls) {
  const p = pathMod.join(tplDirEarly, 'Library', 'ScriptAssemblies', dll);
  if (fs.existsSync(p)) {
    refs += `\n    <Reference Include="${pathMod.basename(dll, '.dll')}">\n      <HintPath>${p}</HintPath>\n    </Reference>`;
  }
}
const analyzers = (tpl.match(/<Analyzer Include="[^"]*"\s*\/>/g) || []).join('\n    ');

// ProjectReference usa caminho relativo a raiz do projeto Unity: converte para absoluto
const tplDir = pathMod.dirname(pathMod.resolve(templateCsproj));
const projRefs = (tpl.match(/<ProjectReference Include="[^"]*"[\s\S]*?<\/ProjectReference>|<ProjectReference Include="[^"]*"\s*\/>/g) || [])
  .map(pr => pr.replace(/Include="([^"]*)"/, (m, p) =>
    'Include="' + pathMod.join(tplDir, p) + '"'))
  .join('\n    ');
const defines = (tpl.match(/<DefineConstants>([\s\S]*?)<\/DefineConstants>/) || [])[1] || '';

const proj = `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <EnableDefaultItems>false</EnableDefaultItems>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <AssemblyName>AnaliseSonar</AssemblyName>
    <OutputType>Library</OutputType>
    <NoStandardLibraries>true</NoStandardLibraries>
    <NoStdLib>true</NoStdLib>
    <NoConfig>true</NoConfig>
    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>
    <NoWarn>0169;USG0001;CS8632</NoWarn>
    <DefineConstants>${defines}</DefineConstants>
    <ErrorLog>${sarifOut},version=2.1</ErrorLog>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <BaseIntermediateOutputPath>obj-analise\\</BaseIntermediateOutputPath>
    <OutputPath>bin-analise\\</OutputPath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SonarAnalyzer.CSharp" Version="10.31.0.145097">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <Compile Include="${targetScripts.replace(/\//g, '\\')}\\**\\*.cs" />
    <!-- Bibliotecas distribuidas como fonte, nao como DLL: necessarias para compilar,
         mas excluidas do relatorio na etapa de filtragem do SARIF -->
${['LeanTween/Framework', 'QuickOutline/Scripts', 'EasyLoadingScreen'].map(d => {
  const p = pathMod.join(pathMod.dirname(pathMod.resolve(targetScripts)), ...d.split('/'));
  return fs.existsSync(p) ? `    <Compile Include="${p}\\**\\*.cs" />` : null;
}).filter(Boolean).join('\n')}
  </ItemGroup>

  <ItemGroup>
    ${analyzers}
  </ItemGroup>

  <ItemGroup>
    ${refs}
  </ItemGroup>

  <ItemGroup>
    ${projRefs}
  </ItemGroup>
</Project>
`;

fs.mkdirSync(pathMod.dirname(outCsproj), { recursive: true });
fs.writeFileSync(outCsproj, proj, 'utf8');
console.log('gerado: ' + outCsproj);
console.log('  referencias : ' + (tpl.match(/<Reference Include=/g) || []).length);
console.log('  analisadores: ' + (tpl.match(/<Analyzer Include=/g) || []).length);
console.log('  fontes      : ' + targetScripts);
