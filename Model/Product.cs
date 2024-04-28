using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendEmailFunction.Model
{
    class Product
    {
        public string productName { get; set; }
        public long productPrice { get; set; }
        public string productDescription { get; set; }
        public long quantity { get; set; }
        public long price { get; set; }
    }
    class OrderDetails
    {
        public string email { get; set; }
        public long userId { get; set; }
        public long totalPrice { get; set; }
        public IList<Product> products { get; set; }
    }
}
