using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class ImageToUpload
    {
        [PrimaryKey]
        public string FileName { get; set; }
    }
}
