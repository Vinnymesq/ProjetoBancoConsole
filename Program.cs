using System;
using System.Collections.Generic;
using System.Linq;

public enum TipoCliente
{
    Comum,
    Super,
    Premium
}

public enum TipoConta
{
    Corrente,
    Poupanca
}

public abstract class Conta
{
    public int contaId { get; set; }
    public string numero { get; set; }
    public decimal saldo { get; protected set; }
    public TipoConta tipo { get; set; }
    public Cliente cliente { get; set; }

    public abstract decimal DescontarTaxa(decimal quantia);
    public abstract void Transferir(decimal quantia);
    public abstract void Depositar(decimal quantia);
}

public class ContaCorrente : Conta
{
    public decimal taxaManutencao { get; set; }

    public override decimal DescontarTaxa(decimal quantia)
    {
        saldo -= taxaManutencao;
        return taxaManutencao;
    }

    public override void Transferir(decimal quantia)
    {
        saldo -= quantia;
    }

    public override void Depositar(decimal quantia)
    {
        saldo += quantia;
    }
}

public class ContaPoupanca : Conta
{
    public decimal taxaRendimento { get; set; }

    public override decimal DescontarTaxa(decimal quantia)
    {
        saldo -= taxaRendimento;
        return taxaRendimento;
    }

    public override void Transferir(decimal quantia)
    {
        saldo -= quantia;
    }

    public override void Depositar(decimal quantia)
    {
        saldo += quantia;
    }
}

public class Cliente
{
    public int id { get; set; }
    public string cpf { get; set; }
    public string nome { get; set; }
    public TipoCliente tipo { get; set; }
    public Conta conta { get; set; }
}

public class Banco
{
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
            Console.WriteLine("Tipo de conta inválido. Por favor, informe 0 para Corrente ou 1 para Poupança:");
        }

        Conta novaConta;
        if (tipoConta == 0)
        {
            novaConta = new ContaCorrente();
        }
        else
        {
            novaConta = new ContaPoupanca();
        }

        // Definindo um número de conta único (pode ser gerado aleatoriamente ou sequencialmente)
        novaConta.numero = GerarNumeroConta();

        // Não definimos o saldo inicial aqui, pois o saldo será atualizado posteriormente com depósitos ou transferências

        Console.WriteLine("=== Cadastro de Cliente ===");
        Console.WriteLine("Informe o CPF do cliente (11 dígitos sem pontos ou traços):");
        string cpf = Console.ReadLine();
        while (cpf.Length != 11 || !cpf.All(char.IsDigit))
        {
            Console.WriteLine("CPF inválido. Por favor, informe os 11 dígitos do CPF:");
            cpf = Console.ReadLine();
        }

        Console.WriteLine("Informe o nome completo do cliente:");
        string nome = Console.ReadLine();

        Cliente novoCliente = new Cliente();
        novoCliente.cpf = cpf;
        novoCliente.nome = nome;
        novoCliente.tipo = TipoCliente.Comum; // Por padrão, o tipo do cliente é Comum

        novaConta.cliente = novoCliente;
        novoCliente.conta = novaConta;

        // Adicionando a nova conta à lista de contas após estar totalmente configurada
        contas.Add(novaConta);

        // Atualizando o tipo do cliente de acordo com o saldo da conta
        AtualizarTipoCliente(novoCliente);

        Console.WriteLine("Conta cadastrada com sucesso!");
    }


    public void TransferirDinheiro()
    {
        Console.WriteLine("=== Transferir Dinheiro ===");

        Console.WriteLine("Informe o número da conta de origem:");
        string numeroOrigem = Console.ReadLine();
        Conta contaOrigem = contas.Find(c => c.numero == numeroOrigem);

        if (contaOrigem == null)
        {
            Console.WriteLine("Conta de origem não encontrada.");
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
    }

    public void DepositarDinheiro()
    {
        Console.WriteLine("=== Depositar Dinheiro ===");

        Console.WriteLine("Informe o número da conta:");
        string numeroConta = Console.ReadLine();
        Conta conta = contas.Find(c => c.numero == numeroConta);

        if (conta == null)
        {
            Console.WriteLine("Conta não encontrada.");
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
    }

    public void ListarNumerosContas()
    {
        Console.WriteLine("=== Números das Contas ===");

        foreach (var conta in contas)
        {
            Console.WriteLine($"Número da Conta: {conta.numero}");
        }
    }

    private string GerarNumeroConta()
    {
        // Implementação para gerar um número de conta único
        // Aqui você pode usar alguma lógica para gerar um número de conta único, como uma sequência numérica, GUID, etc.
        return Guid.NewGuid().ToString().Substring(0, 8); // Exemplo usando GUID (8 primeiros caracteres)
    }


    public void ConsultarSaldo()
    {
        Console.WriteLine("=== Consultar Saldo ===");

        Console.WriteLine("Informe o número da conta:");
        string numeroConta = Console.ReadLine();
        Conta conta = contas.Find(c => c.numero == numeroConta);

        if (conta == null)
        {
            Console.WriteLine("Conta não encontrada.");
            return;
        }

        Console.WriteLine($"Conta encontrada: Número: {conta.numero}, Saldo: {conta.saldo:C}");

        // Restante do código para consultar saldo
    


    // Exibir informações da conta
    Console.WriteLine("=== Dados do Cliente ===");
        Console.WriteLine($"Nome: {conta.cliente.nome}");
        Console.WriteLine($"Tipo de Cliente: {conta.cliente.tipo}");

        Console.WriteLine("=== Dados da Conta ===");
        Console.WriteLine($"Número da Conta: {conta.numero}");
        Console.WriteLine($"Saldo: {conta.saldo:C}");
        Console.WriteLine($"Tipo de Conta: {(conta is ContaCorrente ? "Corrente" : "Poupança")}");

        // Atualizando o tipo do cliente de acordo com o saldo da conta
        AtualizarTipoCliente(conta.cliente);
    }

    public void Sair()
    {
        Console.WriteLine("Encerrando o programa. Obrigado por usar o Sistema Financeiro!");
        Environment.Exit(0);
    }

    private void AtualizarTipoCliente(Cliente cliente)
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

    public void Run()
    {
        bool sair = false;
        Banco banco = new Banco();

        while (!sair)
        {
            Console.WriteLine("Bem-vindo ao Sistema Financeiro!");
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1. Cadastrar nova Conta");
            Console.WriteLine("2. Transferir Dinheiro");
            Console.WriteLine("3. Depositar Dinheiro");
            Console.WriteLine("4. Consultar Saldo");
            Console.WriteLine("5. Listar Números das Contas");
            Console.WriteLine("6. Sair");

            int opcao;
            if (int.TryParse(Console.ReadLine(), out opcao))
            {
                switch (opcao)
                {
                    case 1:
                        banco.CadastrarConta();
                        break;
                    case 2:
                        banco.TransferirDinheiro();
                        break;
                    case 3:
                        banco.DepositarDinheiro();
                        break;
                    case 4:
                        banco.ConsultarSaldo();
                        break;
                    case 5:
                        banco.ListarNumerosContas();
                        break;
                    case 6:
                        banco.Sair();
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

            // Removemos a limpeza da tela aqui

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
