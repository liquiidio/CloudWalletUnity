using System.Collections.Generic;

namespace EosSharp.Core
{
    public class SignedTransaction
    {
        public IEnumerable<string> Signatures { get; set; }
        public byte[] PackedTransaction { get; set; }
    }
}
