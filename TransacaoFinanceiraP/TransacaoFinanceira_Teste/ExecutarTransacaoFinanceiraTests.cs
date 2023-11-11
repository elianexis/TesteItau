using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransacaoFinanceira;

namespace TransacaoFinanceira_Teste
{
    [TestClass]
    public class ExecutarTransacaoFinanceiraTests
    {
        [TestMethod]
        public void TransferenciaComSaldoSuficiente_DeveTransferir()
        {
            // Arrange
            var contaRepository = new ContaRepository();
            var saldoUpdater = new SaldoUpdater();
            var executor = new ExecutarTransacaoFinanceira(contaRepository, saldoUpdater);

            var contaOrigem = new contas_saldo(1, 200);
            var contaDestino = new contas_saldo(2, 100);

            // Atualiza saldos para contas de teste
            contaRepository.Atualizar(contaOrigem);
            contaRepository.Atualizar(contaDestino);

            // Act
            executor.transferir(1, 1, 2, 50);

            // Assert
            Assert.AreEqual(150, contaOrigem.saldo);
            Assert.AreEqual(150, contaDestino.saldo);
        }

        [TestMethod]
        public void TransferenciaComSaldoInsuficiente_DeveCancelarTransferencia()
        {
            // Arrange
            var contaRepository = new ContaRepository();
            var saldoUpdater = new SaldoUpdater();
            var executor = new ExecutarTransacaoFinanceira(contaRepository, saldoUpdater);

            var contaOrigem = new contas_saldo(1, 50);
            var contaDestino = new contas_saldo(2, 100);

            // Atualiza saldos para contas de teste
            contaRepository.Atualizar(contaOrigem);
            contaRepository.Atualizar(contaDestino);

            // Act
            executor.transferir(1, 1, 2, 100);

            // Assert
            // A transferência não deve ocorrer, então os saldos devem permanecer inalterados
            Assert.AreEqual(50, contaOrigem.saldo);
            Assert.AreEqual(100, contaDestino.saldo);
        }
    }
}