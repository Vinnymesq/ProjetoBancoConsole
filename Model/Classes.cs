using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoBancoConsole.Model
{
    public enum TipoCliente
    {
        [EnumMember(Value = "Comum")]
        Comum = 1,

        [EnumMember(Value = "Super")]
        Super = 2,

        [EnumMember(Value = "Premium")]
        Premium = 3
    }

    public enum TipoConta
    {
        [EnumMember(Value = "Corrente")]
        Corrente = 1,

        [EnumMember(Value = "Poupanca")]
        Poupanca = 2
    }
    public abstract class Conta
    {
        public int contaId { get; set; }
        public string numero { get; set; }
        public decimal saldo { get; protected set; }
        public TipoConta tipo { get; set; }
        public Classes cliente { get; set; }

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

    public class Classes
    {
        public string cpf { get; set; }
        public string nome { get; set; }
        public TipoCliente tipo { get; set; }
        public string senha { get; set; }
        public Conta conta { get; set; }
    }
}
