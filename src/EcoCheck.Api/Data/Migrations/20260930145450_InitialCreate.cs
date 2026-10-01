using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EcoCheck.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "survey_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    questionnaire_version = table.Column<int>(type: "integer", nullable: false),
                    total_score = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    classification = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_responses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "question_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    points = table.Column<int>(type: "integer", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_question_options", x => x.id);
                    table.CheckConstraint("ck_question_options_points", "points IS NULL OR (points >= 0 AND points <= 4)");
                    table.ForeignKey(
                        name: "fk_question_options_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "survey_category_scores",
                columns: table => new
                {
                    survey_response_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_category_scores", x => new { x.survey_response_id, x.category });
                    table.ForeignKey(
                        name: "fk_survey_category_scores_survey_responses_survey_response_id",
                        column: x => x.survey_response_id,
                        principalTable: "survey_responses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "survey_answers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    survey_response_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    question_option_id = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_survey_answers_question_options_question_option_id",
                        column: x => x.question_option_id,
                        principalTable: "question_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_answers_survey_responses_survey_response_id",
                        column: x => x.survey_response_id,
                        principalTable: "survey_responses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "questions",
                columns: new[] { "id", "category", "display_order", "is_active", "text" },
                values: new object[,]
                {
                    { 1, "Water", 1, true, "Quanto tempo, em média, dura o seu banho?" },
                    { 2, "Water", 2, true, "Você fecha a torneira enquanto escova os dentes, ensaboa as mãos ou a louça?" },
                    { 3, "Water", 3, true, "Quando aparece um vazamento em casa (torneira pingando, descarga escorrendo), em quanto tempo ele costuma ser resolvido?" },
                    { 4, "Water", 4, true, "Você reaproveita água (da chuva, do enxágue da máquina de lavar etc.) para limpar o chão, regar plantas ou dar descarga?" },
                    { 5, "Water", 5, true, "Como você costuma lavar calçadas, quintais ou veículos?" },
                    { 6, "Energy", 6, true, "Com que frequência você apaga as luzes ao sair de um cômodo?" },
                    { 7, "Energy", 7, true, "Qual tipo de lâmpada predomina onde você mora?" },
                    { 8, "Energy", 8, true, "Com que frequência você deixa aparelhos (TV, computador, videogame, carregadores) ligados ou em espera sem necessidade?" },
                    { 9, "Energy", 9, true, "Ao usar ar-condicionado ou aquecedor, você mantém portas e janelas fechadas e evita temperaturas extremas?" },
                    { 10, "Energy", 10, true, "Ao comprar eletrodomésticos, você verifica a eficiência energética na etiqueta do Inmetro ou o Selo Procel?" },
                    { 11, "Waste", 11, true, "Você separa os materiais recicláveis (papel, plástico, metal, vidro) do lixo comum?" },
                    { 12, "Waste", 12, true, "Como você descarta pilhas, baterias e eletrônicos sem uso?" },
                    { 13, "Waste", 13, true, "Com que frequência você usa itens descartáveis, como copos, talheres, canudos e sacolas plásticas?" },
                    { 14, "Waste", 14, true, "Você reutiliza embalagens, potes, sacolas ou outros materiais antes de descartá-los?" },
                    { 15, "Waste", 15, true, "Você procura evitar o desperdício de alimentos (planejando compras, aproveitando sobras)?" },
                    { 16, "ConsumptionAndMobility", 16, true, "Qual meio de transporte você mais utiliza nos deslocamentos do dia a dia?" },
                    { 17, "ConsumptionAndMobility", 17, true, "Antes de comprar algo novo, você considera se realmente precisa, ou se pode consertar, pegar emprestado ou comprar usado?" },
                    { 18, "ConsumptionAndMobility", 18, true, "Você leva garrafa, caneca ou sacola reutilizável quando sai de casa?" },
                    { 19, "ConsumptionAndMobility", 19, true, "Com que frequência você busca informações sobre o impacto ambiental dos produtos e serviços que consome?" },
                    { 20, "ConsumptionAndMobility", 20, true, "Em passeios por parques, praias, rios ou trilhas, você recolhe seu lixo e evita danificar a natureza?" }
                });

            migrationBuilder.InsertData(
                table: "question_options",
                columns: new[] { "id", "display_order", "points", "question_id", "text" },
                values: new object[,]
                {
                    { 11, 1, 4, 1, "Até 5 minutos" },
                    { 12, 2, 3, 1, "De 5 a 10 minutos" },
                    { 13, 3, 2, 1, "De 10 a 15 minutos" },
                    { 14, 4, 1, 1, "De 15 a 20 minutos" },
                    { 15, 5, 0, 1, "Mais de 20 minutos" },
                    { 21, 1, 4, 2, "Sempre" },
                    { 22, 2, 3, 2, "Frequentemente" },
                    { 23, 3, 2, 2, "Às vezes" },
                    { 24, 4, 1, 2, "Raramente" },
                    { 25, 5, 0, 2, "Nunca" },
                    { 31, 1, 4, 3, "Em poucos dias" },
                    { 32, 2, 2, 3, "Em algumas semanas" },
                    { 33, 3, 1, 3, "Demora meses" },
                    { 34, 4, 0, 3, "Normalmente não é resolvido" },
                    { 35, 5, null, 3, "Nunca percebi vazamentos / não sei" },
                    { 41, 1, 4, 4, "Sempre" },
                    { 42, 2, 3, 4, "Frequentemente" },
                    { 43, 3, 2, 4, "Às vezes" },
                    { 44, 4, 1, 4, "Raramente" },
                    { 45, 5, 0, 4, "Nunca" },
                    { 46, 6, null, 4, "Não tenho essa possibilidade onde moro" },
                    { 51, 1, 4, 5, "Com balde, pano ou vassoura, sem mangueira" },
                    { 52, 2, 3, 5, "Com mangueira com esguicho que interrompe o fluxo" },
                    { 53, 3, 1, 5, "Com mangueira aberta por pouco tempo" },
                    { 54, 4, 0, 5, "Com mangueira aberta durante toda a limpeza" },
                    { 55, 5, null, 5, "Não faço esse tipo de limpeza" },
                    { 61, 1, 4, 6, "Sempre" },
                    { 62, 2, 3, 6, "Frequentemente" },
                    { 63, 3, 2, 6, "Às vezes" },
                    { 64, 4, 1, 6, "Raramente" },
                    { 65, 5, 0, 6, "Nunca" },
                    { 71, 1, 4, 7, "Todas ou quase todas são LED" },
                    { 72, 2, 3, 7, "A maioria é LED" },
                    { 73, 3, 2, 7, "Cerca de metade é LED" },
                    { 74, 4, 1, 7, "Poucas são LED" },
                    { 75, 5, 0, 7, "Nenhuma é LED" },
                    { 76, 6, null, 7, "Não sei" },
                    { 81, 1, 0, 8, "Sempre" },
                    { 82, 2, 1, 8, "Frequentemente" },
                    { 83, 3, 2, 8, "Às vezes" },
                    { 84, 4, 3, 8, "Raramente" },
                    { 85, 5, 4, 8, "Nunca" },
                    { 91, 1, 4, 9, "Sempre" },
                    { 92, 2, 3, 9, "Frequentemente" },
                    { 93, 3, 2, 9, "Às vezes" },
                    { 94, 4, 1, 9, "Raramente" },
                    { 95, 5, 0, 9, "Nunca" },
                    { 96, 6, null, 9, "Não uso ar-condicionado nem aquecedor" },
                    { 101, 1, 4, 10, "Sempre" },
                    { 102, 2, 3, 10, "Frequentemente" },
                    { 103, 3, 2, 10, "Às vezes" },
                    { 104, 4, 1, 10, "Raramente" },
                    { 105, 5, 0, 10, "Nunca" },
                    { 106, 6, null, 10, "Não costumo comprar eletrodomésticos" },
                    { 111, 1, 4, 11, "Sempre" },
                    { 112, 2, 3, 11, "Frequentemente" },
                    { 113, 3, 2, 11, "Às vezes" },
                    { 114, 4, 1, 11, "Raramente" },
                    { 115, 5, 0, 11, "Nunca" },
                    { 121, 1, 4, 12, "Levo a pontos de coleta ou devolvo à loja/fabricante" },
                    { 122, 2, 2, 12, "Guardo até encontrar um ponto de coleta" },
                    { 123, 3, 1, 12, "Às vezes no lixo comum, às vezes em ponto de coleta" },
                    { 124, 4, 0, 12, "No lixo comum" },
                    { 125, 5, null, 12, "Ainda não precisei descartar" },
                    { 131, 1, 0, 13, "Sempre" },
                    { 132, 2, 1, 13, "Frequentemente" },
                    { 133, 3, 2, 13, "Às vezes" },
                    { 134, 4, 3, 13, "Raramente" },
                    { 135, 5, 4, 13, "Nunca" },
                    { 141, 1, 4, 14, "Sempre" },
                    { 142, 2, 3, 14, "Frequentemente" },
                    { 143, 3, 2, 14, "Às vezes" },
                    { 144, 4, 1, 14, "Raramente" },
                    { 145, 5, 0, 14, "Nunca" },
                    { 151, 1, 4, 15, "Sempre" },
                    { 152, 2, 3, 15, "Frequentemente" },
                    { 153, 3, 2, 15, "Às vezes" },
                    { 154, 4, 1, 15, "Raramente" },
                    { 155, 5, 0, 15, "Nunca" },
                    { 161, 1, 4, 16, "A pé ou de bicicleta" },
                    { 162, 2, 3, 16, "Transporte público" },
                    { 163, 3, 2, 16, "Carona ou transporte compartilhado" },
                    { 164, 4, 1, 16, "Carro, moto ou aplicativo individual" },
                    { 165, 5, null, 16, "Quase não me desloco (estudo/trabalho em casa)" },
                    { 171, 1, 4, 17, "Sempre" },
                    { 172, 2, 3, 17, "Frequentemente" },
                    { 173, 3, 2, 17, "Às vezes" },
                    { 174, 4, 1, 17, "Raramente" },
                    { 175, 5, 0, 17, "Nunca" },
                    { 181, 1, 4, 18, "Sempre" },
                    { 182, 2, 3, 18, "Frequentemente" },
                    { 183, 3, 2, 18, "Às vezes" },
                    { 184, 4, 1, 18, "Raramente" },
                    { 185, 5, 0, 18, "Nunca" },
                    { 191, 1, 4, 19, "Sempre" },
                    { 192, 2, 3, 19, "Frequentemente" },
                    { 193, 3, 2, 19, "Às vezes" },
                    { 194, 4, 1, 19, "Raramente" },
                    { 195, 5, 0, 19, "Nunca" },
                    { 201, 1, 4, 20, "Sempre" },
                    { 202, 2, 3, 20, "Frequentemente" },
                    { 203, 3, 2, 20, "Às vezes" },
                    { 204, 4, 1, 20, "Raramente" },
                    { 205, 5, 0, 20, "Nunca" },
                    { 206, 6, null, 20, "Não costumo fazer esse tipo de passeio" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_question_options_question_id",
                table: "question_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_questions_category_display_order",
                table: "questions",
                columns: new[] { "category", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_survey_answers_question_id_question_option_id",
                table: "survey_answers",
                columns: new[] { "question_id", "question_option_id" });

            migrationBuilder.CreateIndex(
                name: "ix_survey_answers_question_option_id",
                table: "survey_answers",
                column: "question_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_answers_survey_response_id_question_id",
                table: "survey_answers",
                columns: new[] { "survey_response_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_survey_category_scores_category",
                table: "survey_category_scores",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_survey_responses_classification",
                table: "survey_responses",
                column: "classification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "survey_answers");

            migrationBuilder.DropTable(
                name: "survey_category_scores");

            migrationBuilder.DropTable(
                name: "question_options");

            migrationBuilder.DropTable(
                name: "survey_responses");

            migrationBuilder.DropTable(
                name: "questions");
        }
    }
}
