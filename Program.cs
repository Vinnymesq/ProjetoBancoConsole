using Microsoft.Data.SqlClient;
using ProjetoBancoConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


public class Banco
{
    private SqlConnection com = new SqlConnection(@"Server=localhost\MSSQLSERVER01;Database=BancoFinanceiro;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;");
    private List<Conta> contas;

    public Banco()
    {
        contas = new List<Conta>();
    }

    public void CadastrarConta()
    {
        Console.WriteLine("=== Cadastrar Nova Conta ===");

        Console.WriteLine("Informe o tipo de conta (0 para Corrente, 1 para Poupança):");
        int tipoConta;
        while (!int.TryParse(Console.ReadLine(), out tipoConta) || (tipoConta != 0 && tipoConta != 1))
        {
            Console.WriteLine("Tipo de conta inválido. Por favor, informe 0 para Corrente ou 1 para Poupança");
        }

        Conta novaConta;
        if (tipoConta == 0)
        {
            novaConta = new ContaCorrente();
            ((ContaCorrente)novaConta).tipo = TipoConta.Corrente; // Adicionado
        }
        else
        {
            novaConta = new ContaPoupanca();
            ((ContaPoupanca)novaConta).tipo = TipoConta.Poupanca; // Adicionado
        }

        novaConta.numero = GerarNumeroConta();

        Console.WriteLine("=== Cadastro de Cliente ===");
        Console.WriteLine("Informe o CPF do cliente (11 dígitos sem pontos ou traços):");
        string cpf = Console.ReadLine();
        while (cpf.Length != 11 || !cpf.All(char.IsDigit))
        {
            Console.WriteLine("CPF inválido. Por favor, informe os 11 dígitos do CPF:");
            cpf = Console.ReadLine();
        }

        if (VerificarCPFExistente(cpf))
        {
            Console.WriteLine("CPF já cadastrado em outra conta. Não é possível cadastrar uma nova conta com o mesmo CPF.");
            return;
        }

        Console.WriteLine("Informe o nome completo do cliente:");
        string nome = Console.ReadLine();

        Console.WriteLine("Informe a senha do cliente:");
        string senha = Console.ReadLine();

        Classes novoCliente = new Classes();
        novoCliente.cpf = cpf;
        novoCliente.nome = nome;
        novoCliente.tipo = TipoCliente.Comum;
        novoCliente.senha = senha;
        novaConta.cliente = novoCliente;
        novoCliente.conta = novaConta;

        contas.Add(novaConta);

        InserirClienteNoBanco(novoCliente);
        InserirContaNoBanco(novaConta);

        AtualizarTipoCliente(novoCliente);

        Console.WriteLine("Conta cadastrada com sucesso!");
    }


    public void TransferirDinheiro()
    {
        Console.WriteLine("=== Transferir Dinheiro ===");

        Console.WriteLine("Informe o CPF do cliente:");
        string cpf = Console.ReadLine();

        Console.WriteLine("Informe a senha:");
        string senha = Console.ReadLine();

        Conta contaOrigem = VerificarAutenticidadeCliente(cpf, senha);

        if (contaOrigem == null)
        {
            Console.WriteLine("Cliente não autenticado. Retornando para o menu.");
            return;
        }

        Console.WriteLine($"Conta de origem encontrada: Número: {contaOrigem.numero}, Saldo: {contaOrigem.saldo:C}");

        Console.WriteLine("Informe o número da conta de destino:");
        string numeroDestino = Console.ReadLine();
        Conta contaDestino = contas.Find(c => c.numero == numeroDestino);

        if (contaDestino == null)
        {
            Console.WriteLine("Conta de destino não encontrada.");
            return;
        }

        Console.WriteLine($"Conta de destino encontrada: Número: {contaDestino.numero}, Saldo: {contaDestino.saldo:C}");

        Console.WriteLine("Informe o valor a ser transferido:");
        decimal valorTransferencia;
        while (!decimal.TryParse(Console.ReadLine(), out valorTransferencia) || valorTransferencia <= 0)
        {
            Console.WriteLine("Valor inválido. Por favor, informe um valor válido:");
        }

        if (contaOrigem.saldo < valorTransferencia)
        {
            Console.WriteLine($"Saldo insuficiente na conta de origem. Saldo atual: {contaOrigem.saldo:C}");
            return;
        }

        contaOrigem.Transferir(valorTransferencia);
        contaDestino.Depositar(valorTransferencia);

        Console.WriteLine("Transferência realizada com sucesso!");
        Console.WriteLine($"Novo saldo na conta de origem: {contaOrigem.saldo:C}");

        AtualizarInformacoesBancoDeDados(contaOrigem, contaDestino);
        AtualizarTipoCliente(contaOrigem.cliente);
        AtualizarTipoCliente(contaDestino.cliente);
    }

    public void DepositarDinheiro()
    {
        Console.WriteLine("=== Depositar Dinheiro ===");

        Console.WriteLine("Informe o CPF do cliente:");
        string cpf = Console.ReadLine();

        Console.WriteLine("Informe a senha:");
        string senha = Console.ReadLine();

        Conta conta = VerificarAutenticidadeCliente(cpf, senha);

        if (conta == null)
        {
            Console.WriteLine("Cliente não autenticado. Retornando para o menu.");
            return;
        }

        Console.WriteLine($"Conta encontrada: Número: {conta.numero}, Saldo: {conta.saldo:C}");

        Console.WriteLine("Informe o valor a ser depositado:");
        decimal valorDeposito;
        while (!decimal.TryParse(Console.ReadLine(), out valorDeposito) || valorDeposito <= 0)
        {
            Console.WriteLine("Valor inválido. Por favor, informe um valor válido:");
        }

        conta.Depositar(valorDeposito);

        Console.WriteLine("Depósito realizado com sucesso!");
        Console.WriteLine($"Novo saldo na conta: {conta.saldo:C}");

        AtualizarInformacoesBancoDeDados(conta);
        AtualizarTipoCliente(conta.cliente);
    }

    public void ListarNumerosContas()
    {
        Console.WriteLine("=== Números das Contas ===");

        foreach (var conta in contas)
        {
            Console.WriteLine($"Número da Conta: {conta.numero}");
        }
    }

    public void ConsultarSaldo()
    {
        Console.WriteLine("=== Consultar Saldo ===");

        Console.WriteLine("Informe o CPF do cliente:");
        string cpf = Console.ReadLine();

        Console.WriteLine("Informe a senha:");
        string senha = Console.ReadLine();

        Conta conta = VerificarAutenticidadeCliente(cpf, senha);

        if (conta == null)
        {
            Console.WriteLine("Cliente não autenticado. Retornando para o menu.");
            return;
        }

        Console.WriteLine($"Conta encontrada: Número: {conta.numero}, Saldo: {conta.saldo:C}");

        Console.WriteLine($"Tipo de Cliente: {ObterTipoCliente(conta.cliente)}");

        ExibirInformacoesClienteEConta(conta.cliente, conta);

        AtualizarTipoCliente(conta.cliente);
    }

    public void RemoverClienteEConta()
    {
        Console.WriteLine("=== Remover Cliente e Conta ===");

        Console.WriteLine("Informe o CPF do cliente:");
        string cpf = Console.ReadLine();

        Console.WriteLine("Informe a senha:");
        string senha = Console.ReadLine();

        Conta conta = VerificarAutenticidadeCliente(cpf, senha);

        if (conta == null)
        {
            Console.WriteLine("Cliente não autenticado. Retornando para o menu.");
            return;
        }

        Console.WriteLine("Tem certeza que deseja remover o cliente e a conta? (s/n)");
        string resposta = Console.ReadLine();

        if (resposta.ToLower() == "s")
        {
            RemoverClienteEContaDoBancoDeDados(conta);
            contas.Remove(conta);
            Console.WriteLine("Cliente e conta removidos com sucesso.");
        }
        else
        {
            Console.WriteLine("Remoção cancelada. Retornando para o menu.");
        }
    }

    private string GerarNumeroConta()
    {
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    private bool VerificarCPFExistente(string cpf)
    {
        return contas.Any(c => c.cliente.cpf == cpf);
    }

    private void InserirClienteNoBanco(Classes cliente)
    {
        try
        {
            com.Open();

            string query = $"INSERT INTO Clientes (cpf, nome, tipoCliente, senha) VALUES (@cpf, @nome, @tipoCliente, @senha)";

            using (SqlCommand command = new SqlCommand(query, com))
            {

                command.Parameters.AddWithValue("@cpf", cliente.cpf);
                command.Parameters.AddWithValue("@nome", cliente.nome);
                command.Parameters.AddWithValue("@tipoCliente", (int)cliente.tipo);
                command.Parameters.AddWithValue("@senha", cliente.senha);

                command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao inserir cliente no banco de dados: " + ex.Message);
        }
        finally
        {
            com.Close();
        }
    }

    private void InserirContaNoBanco(Conta conta)
{
    try
    {
        com.Open();

        string query = $"INSERT INTO Contas (numero, saldo, tipoConta, clienteCpf) VALUES (@numero, @saldo, @tipoConta, @clienteCpf)";

        // Convertendo o tipo de conta para string antes de inserir no banco de dados
        string tipoContaString = "";
        switch (conta.tipo)
        {
            case TipoConta.Corrente:
                tipoContaString = "Corrente";
                break;
            case TipoConta.Poupanca:
                tipoContaString = "Poupanca";
                break;
            default:
                tipoContaString = "Corrente"; // Definindo um padrão caso o tipo de conta seja desconhecido
                break;
        }

        using (SqlCommand command = new SqlCommand(query, com))
        {
            command.Parameters.AddWithValue("@numero", conta.numero);
            command.Parameters.AddWithValue("@saldo", conta.saldo);
            command.Parameters.AddWithValue("@tipoConta", tipoContaString); // Aqui estamos passando o tipo de conta como string
            command.Parameters.AddWithValue("@clienteCpf", conta.cliente.cpf);

            command.ExecuteNonQuery();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro ao inserir conta no banco de dados: " + ex.Message);
    }
    finally
    {
        com.Close();
    }
}

    private Conta VerificarAutenticidadeCliente(string cpf, string senha)
    {
        Conta conta = contas.Find(c => c.cliente.cpf == cpf);

        if (conta != null && conta.cliente.senha == senha)
        {
            return conta;
        }

        return null;
    }

    

    private string ObterTipoCliente(Classes cliente)
    {
        if (cliente.conta.saldo >= 15000)
        {
            return "Premium";
        }
        else if (cliente.conta.saldo >= 5000)
        {
            return "Super";
        }
        else
        {
            return "Comum";
        }
    }

    private void ExibirInformacoesClienteEConta(Classes cliente, Conta conta)
    {
        Console.WriteLine("=== Dados do Cliente ===");
        Console.WriteLine($"Nome: {cliente.nome}");
        Console.WriteLine($"Tipo de Cliente: {ObterTipoCliente(cliente)}");

        Console.WriteLine("=== Dados da Conta ===");
        Console.WriteLine($"Número da Conta: {conta.numero}");
        Console.WriteLine($"Saldo: {conta.saldo:C}");
        Console.WriteLine($"Tipo de Conta: {(conta is ContaCorrente ? "Corrente" : "Poupança")}");
    }

    private void AtualizarTipoCliente(Classes cliente)
    {
        if (cliente.conta.saldo >= 15000)
        {
            cliente.tipo = TipoCliente.Premium;
        }
        else if (cliente.conta.saldo >= 5000)
        {
            cliente.tipo = TipoCliente.Super;
        }
        else
        {
            cliente.tipo = TipoCliente.Comum;
        }
    }

    private void AtualizarInformacoesBancoDeDados(params Conta[] contas)
    {
        try
        {
            com.Open();

            foreach (var conta in contas)
            {
                // Exemplo de comando SQL para atualizar o saldo da conta
                string query = $"UPDATE Contas SET saldo = {conta.saldo} WHERE numero = '{conta.numero}'";

                using (SqlCommand command = new SqlCommand(query, com))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao atualizar informações no banco de dados: " + ex.Message);
        }
        finally
        {
            com.Close();
        }
    }

    private void RemoverClienteEContaDoBancoDeDados(Conta conta)
    {
        try
        {
            com.Open();

            // Exclua a conta associada ao cliente
            string queryRemoverConta = $"DELETE FROM Contas WHERE clienteCPF = '{conta.cliente.cpf}'";

            using (SqlCommand commandRemoverConta = new SqlCommand(queryRemoverConta, com))
            {
                commandRemoverConta.ExecuteNonQuery();
            }

            // Em seguida, remova o cliente
            string queryRemoverCliente = $"DELETE FROM Clientes WHERE cpf = '{conta.cliente.cpf}'";

            using (SqlCommand commandRemoverCliente = new SqlCommand(queryRemoverCliente, com))
            {
                commandRemoverCliente.ExecuteNonQuery();
            }

            Console.WriteLine("Cliente e conta removidos com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao remover cliente e conta do banco de dados: " + ex.Message);
        }
        finally
        {
            com.Close();
        }
    }
    public void Sair()
    {
        Console.WriteLine("Encerrando o programa. Obrigado por usar o Sistema Financeiro!");
        Environment.Exit(0);
    }

    public void Run()
    {
        bool sair = false;

        while (!sair)
        {
            Console.WriteLine("Bem-vindo ao Sistema Financeiro!");
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1. Cadastrar nova Conta");
            Console.WriteLine("2. Transferir Dinheiro");
            Console.WriteLine("3. Depositar Dinheiro");
            Console.WriteLine("4. Consultar Saldo");
            Console.WriteLine("5. Listar Números das Contas");
            Console.WriteLine("6. Remover Cliente e Conta");
            Console.WriteLine("7. Sair");

            int opcao;
            if (int.TryParse(Console.ReadLine(), out opcao))
            {
                switch (opcao)
                {
                    case 1:
                        CadastrarConta();
                        break;
                    case 2:
                        TransferirDinheiro();
                        break;
                    case 3:
                        DepositarDinheiro();
                        break;
                    case 4:
                        ConsultarSaldo();
                        break;
                    case 5:
                        ListarNumerosContas();
                        break;
                    case 6:
                        RemoverClienteEConta();
                        break;
                    case 7:
                        Sair();
                        sair = true;
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Por favor, escolha uma opção válida.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Opção inválida. Por favor, escolha uma opção válida.");
            }

            Console.WriteLine("\nPressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
    }

    static void Main(string[] args)
    {
        Banco banco = new Banco();
        banco.Run();
    }
}
