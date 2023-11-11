using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace TransacaoFinanceira
{
    #region Interface
    public interface ISaldoUpdater
    {
        void AtualizarSaldo(contas_saldo conta, decimal valor);
    }

    public interface IContaRepository<T>
    {
        T ObterSaldo(int id);
        bool Atualizar(T dado);
    }

    #endregion
    public class contas_saldo
    {
        public contas_saldo(int conta, decimal valor)
        {
            this.conta = conta;
            this.saldo = valor;
        }

        public int conta { get; set; }
        public decimal saldo { get; set; }
    }

    public class SaldoUpdater : ISaldoUpdater
    {
        public void AtualizarSaldo(contas_saldo conta, decimal valor)
        {
            conta.saldo += valor;
        }
    }   

    public class ContaRepository : IContaRepository<contas_saldo>
    {
        private readonly object lockObj = new object();
        private readonly List<contas_saldo> TABELA_SALDOS;
        Dictionary<int, decimal> SALDOS { get; set; }

        public ContaRepository()
        {
            TABELA_SALDOS = new List<contas_saldo>();
            TABELA_SALDOS.Add(new contas_saldo(938485762, 180));
            TABELA_SALDOS.Add(new contas_saldo(347586970, 1200));
            TABELA_SALDOS.Add(new contas_saldo(214748364, 0));
            TABELA_SALDOS.Add(new contas_saldo(675869708, 4900));
            TABELA_SALDOS.Add(new contas_saldo(238596054, 478));
            TABELA_SALDOS.Add(new contas_saldo(573659065, 787));
            TABELA_SALDOS.Add(new contas_saldo(210385733, 10));
            TABELA_SALDOS.Add(new contas_saldo(674038564, 400));
            TABELA_SALDOS.Add(new contas_saldo(563856300, 1200));

            SALDOS = new Dictionary<int, decimal>()
                    {
                        { 938485762, 180 }
                    };
        }

        public contas_saldo ObterSaldo(int id)
        {
            lock (lockObj)
            {
                return TABELA_SALDOS.Find(x => x.conta == id);
            }
        }

        public bool Atualizar(contas_saldo dado)
        {
            lock (lockObj)
            {
                try
                {
                    TABELA_SALDOS.RemoveAll(x => x.conta == dado.conta);
                    TABELA_SALDOS.Add(dado);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }
    }

    public class ExecutarTransacaoFinanceira
    {
        private readonly IContaRepository<contas_saldo> contaRepository;
        private readonly ISaldoUpdater saldoUpdater;

        public ExecutarTransacaoFinanceira(IContaRepository<contas_saldo> contaRepository, ISaldoUpdater saldoUpdater)
        {
            this.contaRepository = contaRepository;
            this.saldoUpdater = saldoUpdater;
        }

        public void transferir(int correlation_id, int conta_origem, int conta_destino, decimal valor)
        {
            var conta_saldo_origem = contaRepository.ObterSaldo(conta_origem);
            var conta_saldo_destino = contaRepository.ObterSaldo(conta_destino);

            if (conta_saldo_origem.saldo < valor)
            {
                Console.WriteLine($"Transacao numero {correlation_id} foi cancelada por falta de saldo");
            }
            else
            {
                saldoUpdater.AtualizarSaldo(conta_saldo_origem, -valor);
                saldoUpdater.AtualizarSaldo(conta_saldo_destino, valor);
                Console.WriteLine($"Transacao numero {correlation_id} foi efetivada com sucesso! Novos saldos: Conta Origem:{conta_saldo_origem.saldo} | Conta Destino: {conta_saldo_destino.saldo}");
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var contaRepository = new ContaRepository();
            var saldoUpdater = new SaldoUpdater();
            var executor = new ExecutarTransacaoFinanceira(contaRepository, saldoUpdater);

            var TRANSACOES = new[]
            {
                new {correlation_id= 1, conta_origem= 938485762, conta_destino= 214748364, VALOR= 150},
                new {correlation_id= 2, conta_origem= 214748364, conta_destino= 210385733, VALOR= 149},
                new {correlation_id= 3, conta_origem= 347586970, conta_destino= 238596054, VALOR= 1100},
                new {correlation_id= 4, conta_origem= 675869708, conta_destino= 210385733, VALOR= 5300},
                new {correlation_id= 5, conta_origem= 238596054, conta_destino= 674038564, VALOR= 1489},
                new {correlation_id= 6, conta_origem= 573659065, conta_destino= 563856300, VALOR= 49},
                new {correlation_id= 7, conta_origem= 938485762, conta_destino= 214748364, VALOR= 44},
                new {correlation_id= 8, conta_origem= 573659065, conta_destino= 675869708, VALOR= 150}
            };

            Parallel.ForEach(TRANSACOES, item =>
            {
                executor.transferir(item.correlation_id, item.conta_origem, item.conta_destino, item.VALOR);
            });
        }
    }
}
