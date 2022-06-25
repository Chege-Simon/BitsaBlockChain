
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EllipticCurve;

namespace BlockchainCore
{
    public class Load
    {
        public Transaction Transaction { get; set; }
        public Signature Signature { get; set; }
        public PublicKey Pubkey { get; set; }
        public string trxHash { get; set; }
    }
}
