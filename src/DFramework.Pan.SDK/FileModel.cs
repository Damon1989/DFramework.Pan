using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFramework.Pan.SDK
{
    public class FileModel : NodeModel
    {
        public int Size { get; set; }
        public string Host { get; set; }

        public string Url
        {
            get
            {
                if (Path == null)//Isolate
                {
                    return $"{Host}Isolate/{Id}";//直接id访问的url,下载地址
                }

                return $"{Host}File/{OwnerId}{Path}{Name}";//path访问的url,File改成Download为下载地址
            }
        }

        public string DownloadUrl
        {
            get
            {
                if (Path == null)
                {
                    return Url;
                }

                return $"{Host}Download./{OwnerId}{Path}{Name}";
            }
        }

        public string ThumbUrl
        {
            get
            {
                if (Path == null)
                {
                    return string.Empty;
                }
                return Host + "Thumb/" + OwnerId + Path + Name + "?w={0}&h={1}";
            }
        }
    }
}