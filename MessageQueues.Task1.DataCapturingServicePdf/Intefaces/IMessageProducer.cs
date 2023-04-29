using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueues.Task1.DataCapturingServicePdf.Intefaces
{
    public interface IMessageProducer
    {
        void SendBytes(string fileName, IList<byte[]> fileBytes);
    }
}
