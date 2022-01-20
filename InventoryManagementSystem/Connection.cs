using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem
{
    public static class Connection
    {
        private static string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string subFolderPath = Path.Combine(path, @"Inventory Managment System\Data\Database\dbIMS.mdf");
        public static string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="+subFolderPath+";Integrated Security=True;Connect Timeout=30";
        
    }
}
