using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFramework.Pan.SDK
{
    public class VirtualFolderModel
    {
        public IEnumerable<FolderModel> SubFolders { get; set; }
        public IEnumerable<FileModel> Files { get; set; }

        public bool IsEmpty { get; set; }
    }
}