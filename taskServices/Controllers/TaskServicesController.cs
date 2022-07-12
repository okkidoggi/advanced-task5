using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using taskService.Models;

namespace taskService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskServicesController : ControllerBase
    {
        private readonly TaskDBContext _context;

        public TaskServicesController(TaskDBContext context)
        {
            _context = context;
        }

        // GET: api/TaskServices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskService>>> GetTasksService()
        {
            return await _context.TasksService.ToListAsync();
        }

        // GET: api/TaskServices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskService>> GetTaskService(int id)
        {
            var taskService = await _context.TasksService.FindAsync(id);

            if (taskService == null)
            {
                return NotFound();
            }

            return taskService;
        }

        // PUT: api/TaskServices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskService(int id, TaskService taskService)
        {
            if (id != taskService.taskId)
            {
                return BadRequest();
            }

            _context.Entry(taskService).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskServiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TaskServices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TaskService>> PostTaskService(TaskService taskService)
        {
            _context.TasksService.Add(taskService);
            await _context.SaveChangesAsync();

            var factory = new ConnectionFactory()
            {   //HostName = "localhost" , 
                //Port = 31672,
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "tasks",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string msg = ", Task Id:"+taskService.taskId+ 
                                    ", task Description:" +taskService.taskDescription+
                                        ", task Priority:"+taskService.taskPriority+ 
                                            ", task Status:" +taskService.taskStatus+ 
                                                ",customer Id:" +taskService.customerId;
                var body = Encoding.UTF8.GetBytes(msg);

                channel.BasicPublish(exchange: "",
                                     routingKey: "tasks",
                                     basicProperties: null,
                                     body: body);
            }
           
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "taskStatus",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = "Task Status:" + taskService.taskStatus;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "taskStatus",
                                     basicProperties: null,
                                     body: body);
            }

            return CreatedAtAction("GetTaskService", new { id = taskService.taskId }, taskService);
        }

        // DELETE: api/TaskServices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskService(int id)
        {
            var taskService = await _context.TasksService.FindAsync(id);
            if (taskService == null)
            {
                return NotFound();
            }

            _context.TasksService.Remove(taskService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskServiceExists(int id)
        {
            return _context.TasksService.Any(e => e.taskId == id);
        }
    }
}
