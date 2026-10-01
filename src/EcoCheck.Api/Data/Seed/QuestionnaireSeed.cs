using EcoCheck.Api.Entities;

namespace EcoCheck.Api.Data.Seed;

/// <summary>
/// Perguntas e alternativas iniciais do EcoCheck (versão 1 do questionário).
/// Cada alternativa possui sua própria pontuação (0 a 4); null = "Não se aplica".
/// Ids das alternativas seguem a convenção: questionId * 10 + ordem.
/// </summary>
public static class QuestionnaireSeed
{
    private sealed record SeedOption(string Text, int? Points);

    private sealed record SeedQuestion(int Id, Category Category, string Text, SeedOption[] Options);

    private static readonly SeedOption[] PositiveFrequency =
    [
        new("Sempre", 4),
        new("Frequentemente", 3),
        new("Às vezes", 2),
        new("Raramente", 1),
        new("Nunca", 0)
    ];

    // Para hábitos negativos: quanto menos frequente, maior a pontuação.
    private static readonly SeedOption[] NegativeFrequency =
    [
        new("Sempre", 0),
        new("Frequentemente", 1),
        new("Às vezes", 2),
        new("Raramente", 3),
        new("Nunca", 4)
    ];

    private static SeedOption[] PositiveFrequencyWith(string notApplicableText) =>
        [.. PositiveFrequency, new(notApplicableText, null)];

    private static readonly SeedQuestion[] Data =
    [
        // Água
        new(1, Category.Water, "Quanto tempo, em média, dura o seu banho?",
        [
            new("Até 5 minutos", 4),
            new("De 5 a 10 minutos", 3),
            new("De 10 a 15 minutos", 2),
            new("De 15 a 20 minutos", 1),
            new("Mais de 20 minutos", 0)
        ]),
        new(2, Category.Water, "Você fecha a torneira enquanto escova os dentes, ensaboa as mãos ou a louça?",
            PositiveFrequency),
        new(3, Category.Water, "Quando aparece um vazamento em casa (torneira pingando, descarga escorrendo), em quanto tempo ele costuma ser resolvido?",
        [
            new("Em poucos dias", 4),
            new("Em algumas semanas", 2),
            new("Demora meses", 1),
            new("Normalmente não é resolvido", 0),
            new("Nunca percebi vazamentos / não sei", null)
        ]),
        new(4, Category.Water, "Você reaproveita água (da chuva, do enxágue da máquina de lavar etc.) para limpar o chão, regar plantas ou dar descarga?",
            PositiveFrequencyWith("Não tenho essa possibilidade onde moro")),
        new(5, Category.Water, "Como você costuma lavar calçadas, quintais ou veículos?",
        [
            new("Com balde, pano ou vassoura, sem mangueira", 4),
            new("Com mangueira com esguicho que interrompe o fluxo", 3),
            new("Com mangueira aberta por pouco tempo", 1),
            new("Com mangueira aberta durante toda a limpeza", 0),
            new("Não faço esse tipo de limpeza", null)
        ]),

        // Energia
        new(6, Category.Energy, "Com que frequência você apaga as luzes ao sair de um cômodo?",
            PositiveFrequency),
        new(7, Category.Energy, "Qual tipo de lâmpada predomina onde você mora?",
        [
            new("Todas ou quase todas são LED", 4),
            new("A maioria é LED", 3),
            new("Cerca de metade é LED", 2),
            new("Poucas são LED", 1),
            new("Nenhuma é LED", 0),
            new("Não sei", null)
        ]),
        new(8, Category.Energy, "Com que frequência você deixa aparelhos (TV, computador, videogame, carregadores) ligados ou em espera sem necessidade?",
            NegativeFrequency),
        new(9, Category.Energy, "Ao usar ar-condicionado ou aquecedor, você mantém portas e janelas fechadas e evita temperaturas extremas?",
            PositiveFrequencyWith("Não uso ar-condicionado nem aquecedor")),
        new(10, Category.Energy, "Ao comprar eletrodomésticos, você verifica a eficiência energética na etiqueta do Inmetro ou o Selo Procel?",
            PositiveFrequencyWith("Não costumo comprar eletrodomésticos")),

        // Resíduos
        new(11, Category.Waste, "Você separa os materiais recicláveis (papel, plástico, metal, vidro) do lixo comum?",
            PositiveFrequency),
        new(12, Category.Waste, "Como você descarta pilhas, baterias e eletrônicos sem uso?",
        [
            new("Levo a pontos de coleta ou devolvo à loja/fabricante", 4),
            new("Guardo até encontrar um ponto de coleta", 2),
            new("Às vezes no lixo comum, às vezes em ponto de coleta", 1),
            new("No lixo comum", 0),
            new("Ainda não precisei descartar", null)
        ]),
        new(13, Category.Waste, "Com que frequência você usa itens descartáveis, como copos, talheres, canudos e sacolas plásticas?",
            NegativeFrequency),
        new(14, Category.Waste, "Você reutiliza embalagens, potes, sacolas ou outros materiais antes de descartá-los?",
            PositiveFrequency),
        new(15, Category.Waste, "Você procura evitar o desperdício de alimentos (planejando compras, aproveitando sobras)?",
            PositiveFrequency),

        // Consumo e Mobilidade
        new(16, Category.ConsumptionAndMobility, "Qual meio de transporte você mais utiliza nos deslocamentos do dia a dia?",
        [
            new("A pé ou de bicicleta", 4),
            new("Transporte público", 3),
            new("Carona ou transporte compartilhado", 2),
            new("Carro, moto ou aplicativo individual", 1),
            new("Quase não me desloco (estudo/trabalho em casa)", null)
        ]),
        new(17, Category.ConsumptionAndMobility, "Antes de comprar algo novo, você considera se realmente precisa, ou se pode consertar, pegar emprestado ou comprar usado?",
            PositiveFrequency),
        new(18, Category.ConsumptionAndMobility, "Você leva garrafa, caneca ou sacola reutilizável quando sai de casa?",
            PositiveFrequency),
        new(19, Category.ConsumptionAndMobility, "Com que frequência você busca informações sobre o impacto ambiental dos produtos e serviços que consome?",
            PositiveFrequency),
        new(20, Category.ConsumptionAndMobility, "Em passeios por parques, praias, rios ou trilhas, você recolhe seu lixo e evita danificar a natureza?",
            PositiveFrequencyWith("Não costumo fazer esse tipo de passeio"))
    ];

    public static IReadOnlyList<Question> Questions { get; } = Data
        .Select(q => new Question
        {
            Id = q.Id,
            Category = q.Category,
            Text = q.Text,
            DisplayOrder = q.Id,
            IsActive = true
        })
        .ToList();

    public static IReadOnlyList<QuestionOption> Options { get; } = Data
        .SelectMany(q => q.Options.Select((o, index) => new QuestionOption
        {
            Id = q.Id * 10 + index + 1,
            QuestionId = q.Id,
            Text = o.Text,
            Points = o.Points,
            DisplayOrder = index + 1
        }))
        .ToList();
}
