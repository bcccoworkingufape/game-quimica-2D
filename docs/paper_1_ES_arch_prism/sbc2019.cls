\NeedsTeXFormat{LaTeX2e}[1995/12/01]
\ProvidesClass{sbc2019}[2018/11/23, v1.0]

\DeclareOption*{\PassOptionsToClass{\CurrentOption}{article}}
\ProcessOptions\relax

\LoadClass[twoside,twocolumn,10pt,a4paper]{article}



\RequirePackage{amsmath,amssymb}
\RequirePackage{etoolbox}
\RequirePackage[style]{abstract}
\RequirePackage{enumitem}
\RequirePackage[explicit]{titlesec}
\RequirePackage[T1]{fontenc}
\RequirePackage[tracking=smallcaps]{microtype}
\RequirePackage{lmodern}
\RequirePackage[book]{FiraSans}
\RequirePackage{caption}
\RequirePackage{fancyhdr}

\RequirePackage{fontspec}


\RequirePackage[A4,top=.4in,includehead,headsep=20pt,headheight=28pt,left=.70in,textwidth=41.5pc,textheight=60pc,columnsep=18pt,footskip=18pt]{geometry}

%\renewcommand*\ttdefault{cmvtt}
\usepackage{fontspec}
\setmainfont{Times New Roman}

\renewcommand{\title}[2][]{%
  \gdef\@shorttitle{#1}
  \gdef\@title{#2}
}

\renewcommand{\author}[2][]{%
  \gdef\@shortauthor{#1}
  \gdef\@author{#2}
}

\newcommand{\affil}[1]{\par\noindent\affilfont#1\par}

\def\@shortauthor{}
\def\@shorttitle{}
\def\@jyear{}
\def\@jnum{}
\def\@jid{}
\def\@jtitle{}
\def\@engtitle{}
\def\@copyrightstatement{}
\def\@doi{}

\newcommand{\doi}[1]{\gdef\@doi{#1}}
\newcommand{\jyear}[1]{\gdef\@jyear{#1}}
\newcommand{\jid}[1]{\gdef\@jid{#1}}
\newcommand{\jtitle}[1]{\gdef\@jtitle{#1}}
\newcommand{\engtitle}[1]{\gdef\@engtitle{#1}}
\newcommand{\copyrightstatement}[1]{\gdef\@copyrightstatement{#1}}

\RequirePackage{environ}
\NewEnviron{frontmatter}{\twocolumn[\BODY]\saythanks}

\def\titlefont{\fontsize{18}{20}\bfseries\selectfont\raggedright\mathversion{normal}}
\def\engtitlefont{\fontsize{12}{14}\bfseries\selectfont\raggedright\mathversion{normal}}
\def\authorfont{\normalsize\rightskip5.5pc\vskip1.0pt \mathversion{normal}}

%\def\affilfont{\footnotesize\vskip1.5pt \raggedright\mathversion{normal}}

%\usepackage{scrextend}
%\deffootnote{0em}{1.6em}{\thefootnotemark.\enskip
%\deffootnote{1.5em}{1em}{\thefootnotemark.\ }
%\deffootnotemark{\textsuperscript{(\thefootnotemark)}}


\def\abstractfont{\small\rightskip5.5pc}
\def\keywordfont{\small\rightskip5.5pc\itshape}
\def\licensefont{\footnotesize}

%% Section

\usepackage{sectsty}


\def\sectionfont{\fontsize{11}{11}\bfseries\rightskip1pc plus1fill\mathversion{bold}}
\def\subsectionfont{\normalsize\bfseries\rightskip1pc plus1fill\mathversion{bold}}
\def\subsubsectionfont{\normalsize\itshape\rightskip1pc plus1fill\mathversion{normal}}
\def\paragraphfont{\normalsize\itshape\mathversion{normal}}%
\def\subparagraphfont{\normalsize}%
\def\subsubparagraphfont{\normalsize}%
\def\bibliofont{\small}
\def\tabnotefont{\footnotesize}

\renewcommand{\@maketitle}{%
  \rule{\linewidth}{.27pt}\vspace{10.5pt}
  {\titlefont\@title\par}
  \vskip6.7pt
  \vspace{5.5pt}
  {\authorfont\@author\par}
  \vskip2pt
}

\renewcommand{\abstractname}{}


\fancyhf{}
\pagestyle{fancy}
\renewcommand{\headrulewidth}{0pt}
%\fancyhead[LE,RO]{\thepage}
%\fancyhead[LO]{\itshape\@shorttitle}
%\fancyhead[RE]{\itshape\@shortauthor}
\fancyhead[L]{\footnotesize\itshape\@shorttitle}
\fancyhead[R]{\footnotesize\itshape\@shortauthor}
\fancypagestyle{plain}{%
  \fancyhf{}
  \lhead[L]{\footnotesize%
    \emph{\vskip6pt \@jtitle}\ifdefempty{\@jid}{}%
    \ifdefempty{\@doi}{}{doi:\space\@doi}\\%
    \scriptsize \ifdefempty{\@copyrightstatement}{}{\vskip3pt \faCreativeCommons~~\@copyrightstatement.\\}%
  }
% \cfoot{\thepage}
  \cfoot{}
}

\renewcommand{\abstitlestyle}[1]{\textbf{#1}}
\renewcommand{\abstractnamefont}{\bfseries}
\renewcommand{\abstracttextfont}{\abstractfont}
\setlength{\absleftindent}{0pt}
\setlength{\absparindent}{0pt}
\apptocmd{\abstract}{\noindent\ignorespaces}{}{}
\appto{\endabstract}{\vskip4pt}

\newenvironment{keywords}{%
  \noindent\keywordfont\textit{\textbf{Keywords: }}%
}{\vskip6pt \rule{\linewidth}{.27pt}\vskip12pt}
\newenvironment{license}{%
  \noindent\licensefont{\faCreativeCommons}%
}{\vskip6pt \rule{\linewidth}{.27pt}\vskip12pt }

\captionsetup*[figure]{font=footnotesize,labelfont=bf,labelsep=period,skip=2pt}
\captionsetup*[table]{font=small,position=top,labelfont=bf,labelsep=period,skip=2pt}

\newenvironment{acknowledgements}{\section*{Acknowledgements}\small}{}

\titleformat{\section}
  {\sectionfont}
  {\thesection}
  {0.5em}
  {{#1}}

\titleformat{\subsection}
  {\subsectionfont}
  {\thesubsection}
  {0.5em}
  {#1}

\titleformat{\subsubsection}
  {\subsubsectionfont}
  {\thesubsubsection}
  {0.5em}
  {#1}

\titleformat{\paragraph}[runin]
  {\paragraphfont}
  {\theparagraph}
  {0.5em}
  {#1}
  
\titlespacing*{\section}{0pt}{18pt}{6pt}
\titlespacing*{\subsection}{0pt}{18pt}{6pt}
\titlespacing*{\subsubsection}{0pt}{10pt}{2pt}
\titlespacing*{\paragraph}{0pt}{6pt}{.5em}

\apptocmd{\quotation}{\setlength{\parindent}{0pt}%
\noindent\ignorespaces}{}{}
\patchcmd{\quotation}{\rightmargin\leftmargin}{\rightmargin0pt}{}{}
\setlist{noitemsep}
\newlist{unnumlist}{itemize}{1}
\setlist[unnumlist]{label={},leftmargin=2em,itemindent=-1em}

\def\x{@{\extracolsep{\fill}}}

\newcommand{\tabnote}[1]{\par\vskip1pt{\raggedright\tabnotefont #1\par}}

\RequirePackage{natbib}
\setlength{\bibsep}{0.3mm}

\frenchspacing

\addtolength{\textfloatsep}{-0.0cm}
\setlength\textfloatsep{0.2cm}



\endinput