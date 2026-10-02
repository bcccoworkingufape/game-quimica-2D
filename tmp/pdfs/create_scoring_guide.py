from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle,
    PageBreak, KeepTogether
)

OUTPUT = "output/pdf/sistema-de-pontuacao-molz.pdf"

pdfmetrics.registerFont(TTFont("Arial", "C:/Windows/Fonts/arial.ttf"))
pdfmetrics.registerFont(TTFont("ArialBold", "C:/Windows/Fonts/arialbd.ttf"))

styles = getSampleStyleSheet()
title = ParagraphStyle(
    "TitleMolz", parent=styles["Title"], fontName="ArialBold", fontSize=25,
    leading=30, alignment=TA_CENTER, textColor=colors.HexColor("#4A260E"), spaceAfter=10
)
subtitle = ParagraphStyle(
    "Subtitle", parent=styles["Normal"], fontName="Arial", fontSize=11,
    leading=15, alignment=TA_CENTER, textColor=colors.HexColor("#6D4B34"), spaceAfter=18
)
h1 = ParagraphStyle(
    "H1", parent=styles["Heading1"], fontName="ArialBold", fontSize=17,
    leading=21, textColor=colors.HexColor("#6B3514"), spaceBefore=10, spaceAfter=8
)
body = ParagraphStyle(
    "Body", parent=styles["BodyText"], fontName="Arial", fontSize=10.5,
    leading=15, textColor=colors.HexColor("#2F2118"), spaceAfter=8
)
small = ParagraphStyle(
    "Small", parent=body, fontSize=9, leading=12, textColor=colors.HexColor("#604B3C")
)
callout = ParagraphStyle(
    "Callout", parent=body, backColor=colors.HexColor("#FFF3D4"), borderColor=colors.HexColor("#C98E38"),
    borderWidth=0.7, borderPadding=10, spaceBefore=5, spaceAfter=12
)
table_cell = ParagraphStyle(
    "TableCell", parent=body, fontSize=9.5, leading=12, spaceAfter=0
)
table_header = ParagraphStyle(
    "TableHeader", parent=table_cell, fontName="ArialBold", textColor=colors.white,
    alignment=TA_CENTER
)

def para(text, style=body):
    return Paragraph(text, style)

def table(data, widths=None, header=True):
    formatted = []
    for row_index, row in enumerate(data):
        cell_style = table_header if header and row_index == 0 else table_cell
        formatted.append([Paragraph(str(value), cell_style) for value in row])
    t = Table(formatted, colWidths=widths, repeatRows=1 if header else 0, hAlign="LEFT")
    commands = [
        ("FONTNAME", (0, 0), (-1, -1), "Arial"),
        ("FONTSIZE", (0, 0), (-1, -1), 9.5),
        ("LEADING", (0, 0), (-1, -1), 12),
        ("TEXTCOLOR", (0, 0), (-1, -1), colors.HexColor("#2F2118")),
        ("GRID", (0, 0), (-1, -1), 0.35, colors.HexColor("#D6B884")),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("LEFTPADDING", (0, 0), (-1, -1), 7),
        ("RIGHTPADDING", (0, 0), (-1, -1), 7),
        ("TOPPADDING", (0, 0), (-1, -1), 6),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.HexColor("#FFF9EC"), colors.white]),
    ]
    if header:
        commands += [
            ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#6B3514")),
            ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
            ("FONTNAME", (0, 0), (-1, 0), "ArialBold"),
            ("ALIGN", (0, 0), (-1, 0), "CENTER"),
        ]
    t.setStyle(TableStyle(commands))
    return t

def footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(colors.HexColor("#D6B884"))
    canvas.line(doc.leftMargin, 1.25 * cm, A4[0] - doc.rightMargin, 1.25 * cm)
    canvas.setFont("Arial", 8)
    canvas.setFillColor(colors.HexColor("#6D4B34"))
    canvas.drawString(doc.leftMargin, 0.8 * cm, "Molz - Sistema de pontuacao")
    canvas.drawRightString(A4[0] - doc.rightMargin, 0.8 * cm, f"Pagina {doc.page}")
    canvas.restoreState()

doc = SimpleDocTemplate(
    OUTPUT, pagesize=A4, leftMargin=1.7 * cm, rightMargin=1.7 * cm,
    topMargin=1.55 * cm, bottomMargin=1.7 * cm, title="Sistema de Pontuacao - Molz"
)

story = []
story += [
    para("Sistema de pontuacao do Molz", title),
    para("Guia rapido e simples para entender como as estrelas sao calculadas", subtitle),
    para("A ideia do sistema e simples: cada questao tem um valor, o jogador perde pontos quando erra e ganha pequenos bonus quando mantem uma boa sequencia de acertos.", body),
    para("As vidas continuam existindo para controlar a partida, mas elas nao entram diretamente na conta dos pontos ou das estrelas.", callout),
    para("Como os pontos sao montados", h1),
    para("A fase tem 10 questoes. Se todas forem acertadas de primeira, elas somam 100 pontos. O bonus de sequencia pode passar de 100 durante a partida, mas o resultado final sempre fica limitado a 100.", body),
]

points = [["Questao", "Valor"], *[[str(i), f"{v} pontos"] for i, v in enumerate([8, 10, 8, 9, 12, 10, 9, 10, 14, 10], start=1)], ["Total", "100 pontos"]]
story.append(table(points, [4.4 * cm, 5 * cm]))
story += [
    Spacer(1, 10),
    para("O que acontece ao responder", h1),
    table([
        ["Situacao", "Efeito"],
        ["Acertou de primeira", "Ganha todo o valor da questao."],
        ["Errou uma resposta", "Perde 10 pontos e a sequencia de acertos volta para zero."],
        ["Acertou depois de errar", "Pode seguir para a proxima questao, mas essa questao nao entrega pontos."],
        ["Fez 3 acertos seguidos", "Ganha +5 pontos. O mesmo acontece em 6 e 9 acertos seguidos."],
    ], [5.2 * cm, 8.3 * cm]),
]

story += [
    PageBreak(),
    para("Como funcionam as meias estrelas", h1),
    para("As estrelas sao dadas em passos de meia estrela. Isso evita que uma diferenca pequena de pontos pareca uma diferenca grande no resultado.", body),
]

stars = [
    ["Pontos finais", "Resultado"],
    ["100", "5 estrelas"],
    ["90 a 99", "4,5 estrelas"],
    ["80 a 89", "4 estrelas"],
    ["70 a 79", "3,5 estrelas"],
    ["60 a 69", "3 estrelas"],
    ["50 a 59", "2,5 estrelas"],
    ["40 a 49", "2 estrelas"],
    ["30 a 39", "1,5 estrela"],
    ["0 a 29", "1 estrela"],
]
story.append(table(stars, [5.2 * cm, 8.3 * cm]))
story += [
    Spacer(1, 12),
    para("Exemplos para ficar facil", h1),
    table([
        ["Situacao", "Conta simplificada", "Resultado"],
        ["Tudo certo de primeira", "100 + bonus, limitado a 100", "100 pontos - 5 estrelas"],
        ["Errou so a questao 9", "100 - 14 - 10 + bonus", "Normalmente 91 a 96 - 4,5 estrelas"],
        ["Errou as questoes 5 e 9", "100 - 12 - 14 - 20 + bonus", "Normalmente 64 a 69 - 3 estrelas"],
        ["Errou duas questoes leves", "Perde os valores das duas, -20 e ainda pode ter bonus", "Geralmente 3,5 ou 4 estrelas"],
    ], [4.1 * cm, 5.5 * cm, 4.1 * cm]),
    Spacer(1, 10),
    para("Importante: a ordem das questoes pode mudar a sequencia de acertos e, por isso, alterar um pouco o bonus. Mesmo assim, uma unica questao errada nao derruba o jogador para poucas estrelas.", callout),
]

story += [
    PageBreak(),
    para("E os modos facil, medio e dificil?", h1),
    para("Os tres modos usam a mesma tabela de pontos, o mesmo bonus e as mesmas meias estrelas. O que muda sao as vidas.", body),
    table([
        ["Modo", "Vidas", "Quando a fase acaba"],
        ["Facil", "Ilimitadas", "O jogador pode continuar ate concluir as 10 questoes."],
        ["Medio", "3", "Ao perder todas as vidas, a fase termina."],
        ["Dificil", "3", "Ao perder todas as vidas, a fase termina."],
    ], [3.0 * cm, 2.5 * cm, 8.2 * cm]),
    Spacer(1, 14),
    para("Resumo em uma linha", h1),
    para("Acertar de primeira vale os pontos da questao. Errar custa 10 pontos e tira o valor daquela questao. Corrigir permite continuar, mas nao devolve os pontos. A cada tres acertos seguidos, entram mais 5 pontos de bonus.", callout),
    para("Esse formato deixa as questoes com pesos diferentes, mas evita que um unico erro em uma questao mais valiosa destrua toda a classificacao.", body),
]

doc.build(story, onFirstPage=footer, onLaterPages=footer)
