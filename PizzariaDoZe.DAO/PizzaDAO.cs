using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace PizzariaDoZe.DAO
{
    public class Pizza
    {
        public Pizza( int id = 0, string tamanho = "", string categoria = "",  bool comBorda = false, decimal valorBorda = 0, decimal valorTotal = 0)
        {
            ID = id;
            Tamanho = tamanho;
            Categoria = categoria;
            ComBorda = comBorda;
            ValorBorda = valorBorda;
            ValorTotal = valorTotal;
        }
        
        public int ID { get; set; }
        public string Tamanho { get; set; }
        public string Categoria { get; set; }
        public bool ComBorda { get; set; }
        public decimal ValorBorda { get; set; }
        public decimal ValorTotal { get; set; }
    }
    public class PizzaDAO
    {
        private readonly DbProviderFactory factory;
        private string Provider { get; set; }
        private string StringConexao { get; set; }

        public PizzaDAO(string provider, string stringConexao)
        {
            Provider = provider;
            StringConexao = stringConexao;
            factory = DbProviderFactories.GetFactory(Provider);
        }

        public int Inserir(Pizza pizza)
        {
            using var conexao = factory.CreateConnection(); //Cria conexão
            conexao!.ConnectionString = StringConexao; //Atribui a string de conexão
            using var comando = factory.CreateCommand(); //Cria comando
            comando!.Connection = conexao; //Atribui conexão
                                           //Adiciona parâmetro (@campo e valor)
            conexao.Open();

            string auxSQL_ID = Provider.Contains("MySql") ? "SELECT LAST_INSERT_ID();" : "SELECT SCOPE_IDENTITY();";
            //realiza o INSERT e retorna o ID gerado, algo que vai ser necessário na sequencia
            comando.CommandText = @"" +
            "INSERT INTO tb_pizza (tamanho, categoria, com_borda, valor_borda, valor_total) " +
            "VALUES (@tamanho, @categoria, @com_borda, @valor_total, @valor_borda);" +
            auxSQL_ID;
            //executa o comando no banco de dados e captura o ID gerado
            var IdvalorGerado = comando.ExecuteScalar();

            return Convert.ToInt32(IdvalorGerado);
        }
        public DataTable Buscar(Pizza pizza)
        {

            using var conexao = factory.CreateConnection(); //Cria conexão
            conexao!.ConnectionString = StringConexao; //Atribui a string de conexão
            using var comando = factory.CreateCommand(); //Cria comando
            comando!.Connection = conexao; //Atribui conexão
                                           //verifica se tem filtro e personaliza o SQL do filtro
            string auxSqlFiltro = "";

            if (pizza.ID > 0)
            {
                auxSqlFiltro = "WHERE p.id_pizza = " + pizza.ID + " ";
            }
            conexao.Open();
            comando.CommandText =   @" " +
                                    "SELECT id_valor AS ID, " +
                                    "tamanho AS Tamanho, " +
                                    "categoria AS Categoria, " +
                                    "com_borda AS 'Com Borda' " +
                                    "valor_borda AS 'Valor Borda' " +
                                    "valor_total AS 'Valor Pizza', " +
                                    "FROM tb_pizza AS p " +
                                    auxSqlFiltro +
                                    ";";
            //Executa o script na conexão e retorna as linhas afetadas.
            var sdr = comando.ExecuteReader();
            DataTable linhas = new();
            linhas.Load(sdr);

            return linhas;
        }        
        public void Editar(Pizza pizza)
        {
            using var conexao = factory.CreateConnection(); //Cria conexão
            conexao!.ConnectionString = StringConexao; //Atribui a string de conexão
            using var comando = factory.CreateCommand(); //Cria comando
            comando!.Connection = conexao; //Atribui conexão
                                           
            ConverterObjetoParaSql(pizza, comando);//Adiciona parâmetro (@campo e valor)
            conexao.Open();
            // Inicia o controle de Transação LOCAL
            DbTransaction transacao = conexao.BeginTransaction();
            // Associa o command com o controle de Transação
            comando.Transaction = transacao;
            try
            {
                //realiza o UPDATE
                comando.CommandText = @"UPDATE tb_pizza SET tamanho = @tamanho, categoria = @categoria, com_borda = @com_borda, valor_borda = @valor_borda, valor_total = @valor_total " +
                                        "WHERE id_pizza = @id;";
                //executa o comando no banco de dados e captura o ID gerado
                _ = comando.ExecuteNonQuery();
                // Como não ocorreu nenhum erro, confirma as transações através do Commit()
                transacao.Commit();
            }
            catch (Exception ex)
            {
                // Alguns dos comandos SQL acima gerou erro, dessa forma, todos os comandos serão desfeitos através do Rollback()
                transacao.Rollback();
                // retorna uma exceção para quem chamou a execução
                throw new Exception(ex.Message);
            }
        }


        public void Excluir(Pizza pizza)
        {
            using var conexao = factory.CreateConnection(); //Cria conexão
            conexao!.ConnectionString = StringConexao; //Atribui a string de conexão
            using var comando = factory.CreateCommand(); //Cria comando
            comando!.Connection = conexao; //Atribui conexão
                                           //Adiciona parâmetro (@campo e valor)
            ConverterObjetoParaSql(pizza, comando);
            conexao.Open();
            // Inicia o controle de Transação LOCAL
            DbTransaction transacao = conexao.BeginTransaction();
            // Associa o command com o controle de Transação
            comando.Transaction = transacao;
            try
            {
                comando.CommandText = @"DELETE FROM sabores_pizza WHERE pizza_id = @id;";
                _ = comando.ExecuteNonQuery();

                comando.CommandText = @"DELETE FROM lista_pizzas WHERE pizza_id = @id;";
                _ = comando.ExecuteNonQuery();

                comando.CommandText = @"DELETE FROM tb_pizza WHERE id_pizza = @id;";
                _ = comando.ExecuteNonQuery();
                // Como não ocorreu nenhum erro, confirma as transações através do Commit()
                transacao.Commit();
            }
            catch (Exception ex)
            {
                // Alguns dos comandos SQL acima gerou erro, dessa forma, todos os comandos serão desfeitos através do Rollback()
                transacao.Rollback();
                // retorna uma exceção para quem chamou a execução
                throw new Exception(ex.Message);
            }
        }

        private void ConverterObjetoParaSql(Pizza pizza, DbCommand comando)
        {
            var Id = comando.CreateParameter(); Id.ParameterName = "@id"; Id.Value = pizza.ID; comando.Parameters.Add(Id);
            var Tamanho = comando.CreateParameter(); Tamanho.ParameterName = "@tamanho"; Tamanho.Value = pizza.Tamanho; comando.Parameters.Add(Tamanho);
            var Categoria = comando.CreateParameter(); Categoria.ParameterName = "@categoria"; Categoria.Value = pizza.Categoria; comando.Parameters.Add(Categoria);
            var ComBorda = comando.CreateParameter(); ComBorda.ParameterName = "@com_borda"; ComBorda.Value = pizza.ComBorda; comando.Parameters.Add(ComBorda);
            var ValorBorda = comando.CreateParameter(); ValorBorda.ParameterName = "@valor_borda"; ValorBorda.Value = pizza.ValorBorda; comando.Parameters.Add(ValorBorda);
            var ValorTotal = comando.CreateParameter(); ValorTotal.ParameterName = "@valor_total"; ValorTotal.Value = pizza.ValorTotal; comando.Parameters.Add(ValorTotal);
        }
    }
}