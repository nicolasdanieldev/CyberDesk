using System;

namespace CyberDesk.Exceptions
{
    // Exceção própria para erros nas regras de negócio do CyberDesk.
    public class ValidacaoCyberDeskException : Exception
    {
        public ValidacaoCyberDeskException() { }

        public ValidacaoCyberDeskException(string mensagem)
            : base(mensagem) { }

        public ValidacaoCyberDeskException(string mensagem, Exception erroOriginal)
            : base(mensagem, erroOriginal) { }
    }
}
