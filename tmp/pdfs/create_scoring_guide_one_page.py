from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import cm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle

OUTPUT = "output/pdf/sistema-de-pontuacao-molz.pdf"
pdfmetrics.registerFont(TTFont("Arial", "C:/Windows/Fonts/arial.ttf"))
pdfmetrics.registerFont(TTFont("ArialBold", "C:/Windows/Fonts/arialbd.ttf"))

styles = getSampleStyleSheet()
title = ParagraphStyle("title", parent=styles["Title"], fontName="ArialBold", fontSize=23, leading=27,
                       alignment=TA_CENTER, textColor=colors.HexColor("#4A260E"), spaceAfter=5)
subtitle = ParagraphStyle("subtitle", parent=styles["Normal"], fontName="Arial", fontSize=10.5, leading=13,
                          alignment=TA_CENTER, textColor=colors.HexColor("#6D4B34"), spaceAfter=12)
h = ParagraphStyle("heading", parent=styles["Heading2"], fontName="ArialBold", fontSize=14.5, leading=18,
                   textColor=colors.HexColor("#6B3514"), spaceBefore=7, spaceAfter=5)
body = ParagraphStyle("body", parent=styles["BodyText"], fontName="Arial", fontSize=9.4, leading=12.5,
                      textColor=colors.HexColor("#2F2118"), spaceAfter=5)
cell = ParagraphStyle("cell", parent=body, fontSize=8.4, leading=10.5, spaceAfter=0)
header = ParagraphStyle("header", parent=cell, fontName="ArialBold", textColor=colors.white, alignment=TA_CENTER)
callout = ParagraphStyle("callout", parent=body, backColor=colors.HexColor("#FFF3D4"),
                         borderColor=colors.HexColor("#C98E38"), borderWidth=.7, borderPadding=7,
                         spaceBefore=3, spaceAfter=7)

def P(text, style=body):
    return Paragraph(text, style)

def make_table(data, widths):
    formatted = []
    for index, row in enumerate(data):
        formatted.append([P(value, header if index == 0 else cell) for value in row])
    result = Table(formatted, colWidths=widths, hAlign="LEFT")
    result.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#6B3514")),
        ("GRID", (0, 0), (-1, -1), .3, colors.HexColor("#D6B884")),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("LEFTPADDING", (0, 0), (-1, -1), 5), ("RIGHTPADDING", (0, 0), (-1, -1), 5),
        ("TOPPADDING", (0, 0), (-1, -1), 4), ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.HexColor("#FFF9EC"), colors.white]),
    ]))
    return result

def footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(colors.HexColor("#D6B884"))
    canvas.line(doc.leftMargin, 1.1 * cm, A4[0] - doc.rightMargin, 1.1 * cm)
    canvas.setFont("Arial", 8)
    canvas.setFillColor(colors.HexColor("#6D4B34"))
    canvas.drawString(doc.leftMargin, .7 * cm, "Molz - Sistema de pontuação")
    canvas.restoreState()

doc = SimpleDocTemplate(OUTPUT, pagesize=A4, leftMargin=1.55*cm, rightMargin=1.55*cm,
                        topMargin=1.2*cm, bottomMargin=1.45*cm, title="Sistema de Pontuação - Molz")
story = [
    P("Sistema de pontuação do Molz", title),
    P("Resumo rápido de como funcionam pontos, bônus e meias estrelas", subtitle),
    P("A fase possui 10 questões que somam 100 pontos. Os três modos usam a mesma conta; no fácil as vidas são ilimitadas. No médio e difícil, perder as 3 vidas encerra a fase.", body),
    P("<b>Regra principal:</b> acertou de primeira, ganha o valor da questão. Errou, perde 15 pontos e quebra a sequência. Se acertar depois, pode continuar, mas essa questão não vale mais pontos nem entra na sequência.", body),
    P("Valor de cada questão", h),
]

values = [8, 10, 8, 9, 12, 10, 9, 10, 14, 10]
value_rows = [["Questão", "Pontos", "Questão", "Pontos"]]
for i in range(5):
    value_rows.append([str(i+1), str(values[i]), str(i+6), str(values[i+5])])
value_rows.append(["Total", "100", "", ""])
story.append(make_table(value_rows, [3.3*cm, 3.0*cm, 3.3*cm, 3.0*cm]))
story += [
    Spacer(1, 4),
    P("Bônus de sequência", h),
    make_table([
        ["Sequência", "Bônus"],
        ["3 acertos de primeira seguidos", "+5 pontos"],
        ["6 acertos de primeira seguidos", "+5 pontos"],
        ["9 acertos de primeira seguidos", "+5 pontos"],
    ], [5.1*cm, 8.0*cm]),
    Spacer(1, 4),
    P("Resultado final", h),
    make_table([
        ["Pontos", "Estrelas"],
        ["100", "5"], ["90 a 99", "4,5"], ["80 a 89", "4"], ["70 a 79", "3,5"],
        ["60 a 69", "3"], ["50 a 59", "2,5"], ["40 a 49", "2"], ["30 a 39", "1,5"], ["0 a 29", "1"],
    ], [5.1*cm, 8.0*cm]),
    Spacer(1, 6),
    P("<b>Observação:</b> os valores das questões são fixos nesta versão. Ainda será preciso definir quais questões receberão cada valor de acordo com o nível de dificuldade delas.", body),
]
doc.build(story, onFirstPage=footer)
