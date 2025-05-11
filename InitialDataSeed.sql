insert into library.dbo.contributors (firstname, lastname, date_of_birth, external_id, created_at, updated_at, active) values
('Aldous', 'Huxley', convert(datetime,'Jul 26 1894'), newid(), getdate(), null, 1),
('Jostein', 'Gaarder', convert(datetime,'Aug 08 1952'), newid(), getdate(), null, 1),
('George', 'Orwell', convert(datetime,'Jul 25 1903'), newid(), getdate(), null, 1),
('Heloisa', 'Jahn', convert(datetime,'Jan 1 1947'), newid(), getdate(), null, 1),
('Alexandre', 'Hubner', convert(datetime,'Oct 25 1966'), newid(), getdate(), null, 1);

insert into library.dbo.categories (name, description, external_id, created_at, updated_at, active) values
(
    'Science Fiction',
    'fiction based on imagined future scientific or technological advances and major social or environmental changes, frequently portraying space or time travel and life on other planets.',
    newid(),
    getdate(),
    null,
    1
),
(
    'Fiction',
    'Literature in the form of prose that describes imaginary events and people. Something that is invented or untrue.',
    newid(),
    getdate(),
    null,
    1
);

insert into library.dbo.publishers (name, foundation_date, phone_number, email, website, external_id, created_at, updated_at, active) values
('Biblioteca Azul', convert(datetime, 'Jan 1 1921'), '+5501199990101', null, null, newid(), getdate(), null, 1),
('Editora Itatiaia', convert(datetime, 'Jan 1 1954'), null, null, null, newid(), getdate(), null, 1),
('Seguinte', convert(datetime, 'Jan 1 2012'), null, 'seguinte@email.com', null, newid(), getdate(), null, 1),
('Phoenix', convert(datetime, 'Jan 1 1986'), null, null, 'https://www.phoenix.com.ph/', newid(), getdate(), null, 1);

insert into library.dbo.books (title, isbn, description, edition, [language], total_pages, published_date, status, category_id, publisher_id, external_id, created_at, updated_at, active) values
(
    'Admirável mundo novo - Edição especial',
    '9786558301660',
    'Para celebrar os 90 anos da primeira publicação, o livro de Aldous Huxley ganha uma edição de luxo em capa dura com projeto gráfico especial e nova tradução do especialista em ficção-científica, Fabio Fernandes. Além disso, a publicação conta com dois textos extras, um assinado por Ursula K. Le Guin, escritora e admiradora confessa deste livro e de seu autor, e o outro de Samir Machado de Machado, escritor gaúcho, que conta a história das obras de Aldous Huxley e de Admirável mundo novo no Brasil, desde sua primeira edição por aqui.

    Em uma sociedade organizada segundo princípios estritamente científicos, Bernard Marx, um psicólogo, sente-se inadequado quando se compara aos outros seres de sua casta. Ao descobrir uma “reserva histórica” que preserva costumes de uma sociedade anterior – muito semelhante a do leitor – Bernard vai perceber as diferenças entre este mundo e o seu – e a partir de um sentimento de inconformismo, ele desafiará o mundo.',
    1,
    'Portuguese',
    304,
    convert(datetime, 'Sep 23 2022'),
    'Available',
    (select id from library.dbo.categories where name = 'Science Fiction'),
    (select id from library.dbo.publishers where name = 'Biblioteca Azul'),
    newid(),
    getdate(),
    null,
    1
),
(
    'As portas da percepção',
    '9788525060211',
    'De volta às livrarias com novo projeto gráfico e nova tradução, relato de Huxley sobre suas experiências com a mescalina segue atual Um dos livros mais conhecidos de Aldous Huxley, As portas da percepção influenciou gerações ao detalhar o efeito das drogas sobre os sentidos do escritor. Publicado pela primeira vez em 1954, o livro antecipou as experiências psicodélicas que marcaram os escritores da geração Beat e o rock n’ roll na década de 1960. A edição da Biblioteca Azul traz As portas da percepção e Céu e inferno em novas traduções e posfácio do neurocientista Sidarta Ribeiro sobre a morte de Huxley. Com a inteligência característica de sua prosa, Huxley fala sobre suas expectativas ao usar mescalina e descreve suas sensações e pensamentos ao observar objetos cotidianos e ao ouvir música. O escritor conclui que os sentidos servem como um filtro, de forma que as pessoas percebam o necessário para garantir sua sobrevivência, sem contemplar nuances e detalhes da realidade. Huxley também divaga sobre o tempo, as religiões e sobre como a alteração da consciência é usada como uma maneira de alcançar a transcendência. O texto continua provocativo e relevante, o que faz de As portas da percepção uma das obras que marcaram o século XX.',
    1,
    'Portuguese',
    160,
    convert(datetime, 'Oct 1 2015'),
    'Available',
    (select id from library.dbo.categories where name = 'Science Fiction'),
    (select id from library.dbo.publishers where name = 'Biblioteca Azul'),
    newid(),
    getdate(),
    null,
    1
),
(
    'O Despertar do Mundo Novo',
    '9788531902789',
    'Aldous Leonard Huxley nasceu em 26 de julho de 1894, na Inglaterra. Em 1916, publica seu primeiro livro, uma coletânea de poemas. A partir de 1921, sua reputação literária se estabelece, através de Crome yellow. Em seguida, escreve Antic hay (1923), Folhas inúteis (1925) e Contraponto (1928), sátiras onde analisa de modo espirituoso e implacável os dissabores do mundo moderno. No período anterior à Segunda Guerra Mundial, sua obra adquire tons mais sombrios, incluindo o célebre romance Admirável mundo novo (1932), antiutopia que descreve a desumanizada sociedade do futuro, e Sem olhos em Gaza (1936), uma novela pacifista. Em 1937, deixa a Europa e se muda para a Califórnia. Além de ensaios sobre assuntos tanto culturais quanto religiosos, em que se nota a forte influência da mística oriental, Huxley publicou O tempo deve parar (1944), O macaco e a essência (1949), A ilha (1962) e As portas da percepção (1954), onde descreve suas experiências com a mescalina. Aldous Huxley faleceu em 22 de novembro de 1963, curiosamente mesmo dia do assassinato de John Fitzgerald Kennedy.',
    1,
    'Portuguese',
    308,
    convert(datetime, 'Jul 25 2000'),
    'Available',
    (select id from library.dbo.categories where name = 'Science Fiction'),
    (select id from library.dbo.publishers where name = 'Editora Itatiaia'),
    newid(),
    getdate(),
    null,
    1
),
(
    'O Mundo de Sofia',
    '8535921893',
    'Às vésperas de seu aniversário de quinze anos, Sofia Amundsen começa a receber bilhetes e cartões-postais bastante estranhos. Os bilhetes são anônimos e perguntam a Sofia quem é ela e de onde vem o mundo. Os postais são enviados do Líbano, por um major desconhecido, para uma certa Hilde Møller Knag, garota a quem Sofia também não conhece. O mistério dos bilhetes e dos postais é o ponto de partida deste romance fascinante, que vem conquistando milhões de leitores em todos os países e já vendeu mais de 1 milhão de exemplares só no Brasil. De capítulo em capítulo, de “lição” em “lição”, o leitor é convidado a percorrer toda a história da filosofia ocidental, ao mesmo tempo que se vê envolvido por um thriller que toma um rumo surpreendente.',
    1,
    'Portuguese',
    568,
    convert(datetime, 'Nov 19 2012'),
    'Reserved',
    (select id from library.dbo.categories where name = 'Fiction'),
    (select id from library.dbo.publishers where name = 'Seguinte'),
    newid(),
    getdate(),
    null,
    1
),
(
    'The Solitaire Mystery',
    '1857998650',
    'Twelve-year-old Hans Thomas lives alone with his father, a man who likes to give his son lessons about life and has a penchant for philosophy. Hans Thomas'' mother left when he was four (to find'' herself) and the story begins when father and son set off on a trip to Greece, where she now lives, to try to persuade her to come home. En route, in Switzerland, Hans Thomas is given a magnifying glass by a dwarf at a petrol station, and the next day he finds a tiny book in his bread roll which can only be read with a magnifying glass. How did the book come to be there? Why does the dwarf keep showing up? It is all very perplexing and Hans Thomas has enough to cope with, with the daunting prospect of seeing his mother. Now his journey has turned into an encounter with the unfathomable...or does it all have a logical explanation?',
    1,
    'English',
    352,
    convert(datetime, 'Jan 1 1997'),
    'Unavailable',
    (select id from library.dbo.categories where name = 'Fiction'),
    (select id from library.dbo.publishers where name = 'Phoenix'),
    newid(),
    getdate(),
    null,
    1
),
(
    '1984',
    '9788535925890',
    'Um dos romances mais influentes do século XX, 1984 é uma obra-prima de George Orwell. Publicado em 1949, o livro apresenta um futuro distópico em que a liberdade individual é suprimida e a verdade é manipulada pelo governo totalitário. O protagonista, Winston Smith, trabalha no Ministério da Verdade, onde reescreve a história para se adequar à narrativa do Partido. À medida que Winston começa a questionar o regime opressivo e busca a verdade, ele se vê envolvido em uma luta desesperada pela liberdade e pela humanidade. Através de sua prosa incisiva e crítica, Orwell nos alerta sobre os perigos do totalitarismo e da vigilância estatal.',
    1,
    'Portuguese',
    368,
    convert(datetime, 'Jan 1 2017'),
    'Available',
    (select id from library.dbo.categories where name = 'Fiction'),
    (select id from library.dbo.publishers where name = 'Seguinte'),
    newid(),
    getdate(),
    null,
    1
);

insert into library.dbo.book_contributors (book_id, contributor_id, role, created_at, updated_at) values
(
    (select id from library.dbo.books where title = 'Admirável mundo novo - Edição especial'),
    (select id from library.dbo.contributors where firstname = 'Aldous' and lastname = 'Huxley'),
    'MainAuthor',
    getdate(),
    null
),
(
    (select id from library.dbo.books where title = 'As portas da percepção'),
    (select id from library.dbo.contributors where firstname = 'Aldous' and lastname = 'Huxley'),
    'MainAuthor',
    getdate(),
    null
),
(
    (select id from library.dbo.books where title = '1984'),
    (select id from library.dbo.contributors where firstname = 'George' and lastname = 'Orwell'),
    'MainAuthor',
    getdate(),
    null
),
(
    (select id from library.dbo.books where title = '1984'),
    (select id from library.dbo.contributors where firstname = 'Heloisa' and lastname = 'Jahn'),
    'Translator',
    getdate(),
    null
),
(
    (select id from library.dbo.books where title = '1984'),
    (select id from library.dbo.contributors where firstname = 'Alexandre' and lastname = 'Hubner'),
    'Translator',
    getdate(),
    null
)