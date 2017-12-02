using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageClient
{
    public interface IStorageClient
    {
        void StorageMessage(string name, string message);
        void StorageLogin(string name);
        void StorageLogout(string name);
    }
}
