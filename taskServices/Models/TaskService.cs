using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace taskService.Models
{
    public class TaskService
    {
        [Key]
        public int taskId { get; set; }
        public string taskDescription { get; set; }

        public string taskPriority{ get; set; }

        public string taskStatus { get; set; }

        public int customerId { get; set; }
    }
}
