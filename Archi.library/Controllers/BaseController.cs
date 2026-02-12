using Archi.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Archi.Library.Controllers
{
    [ApiController]
    public abstract class BaseController<C, M> : ControllerBase
        where C : BaseDbContext
        where M : BaseModel
    {
        protected readonly C _context;

        public BaseController(C context)
        {
            _context = context;
        }

        // GET ALL
        [HttpGet]
        public virtual ActionResult<IEnumerable<M>> GetAll()
        {
            var entities = _context.Set<M>()
                                   .Where(x => !x.IsDeleted)
                                   .ToList();

            return Ok(entities);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public virtual ActionResult<M> GetById(int id)
        {
            var entity = _context.Set<M>().Find(id);

            if (entity == null || entity.IsDeleted)
                return NotFound();

            return Ok(entity);
        }

        // POST
        [HttpPost]
        public virtual ActionResult<M> Post([FromBody] M entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Set<M>().Add(entity);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById),
                new { id = entity.Id },
                entity);
        }

        // PUT
        [HttpPut("{id}")]
        public virtual ActionResult Put(int id, [FromBody] M entity)
        {
            if (id != entity.Id)
                return BadRequest();

            var existing = _context.Set<M>().Find(id);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(existing).CurrentValues.SetValues(entity);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE (Soft delete via BaseDbContext)
        [HttpDelete("{id}")]
        public virtual ActionResult Delete([FromRoute]int id)
        {
            var entity = _context.Set<M>().Find(id);
            if (entity == null)
            {
            return NotFound();
            }

            _context.Set<M>().Remove(entity); // Soft delete auto via BaseDbContext
            _context.SaveChanges();

            return NoContent();
        }
    }
}