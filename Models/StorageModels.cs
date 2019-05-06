using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AwsStorage.Models
{
    [DataContract]
    public class ListModel
    {
        [Required]
        [DataMember]
        public string Prefix;

        [DataMember]
        public int Limit;
    }
}
