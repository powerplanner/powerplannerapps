using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PowerPlannerAppDataLibrary.DataLayer
{
    public class ImageToUpload
    {
        [Key]
        public string FileName { get; set; }
    }
}
