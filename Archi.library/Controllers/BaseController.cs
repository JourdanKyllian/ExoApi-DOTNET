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
        // Appel : GET /api/v1/Pizza (Renvoie 0-25 par défaut AVEC headers)
        // Appel : GET /api/v1/Pizza?range=0-50 (Renvoie 0-50 AVEC headers)
        [HttpGet]
        public virtual ActionResult<IEnumerable<M>> GetAll([FromQuery] string range = "0-25")
        {
            // Préparation de la requête (exclus les résultat Soft Deleted)
            var query = _context.Set<M>().Where(x => !x.IsDeleted);

            // Traitement du Range (Valeur par défaut "0-25" utilisée si range est vide)
            var rangeValues = range.Split('-');
            int start = 0;
            int end = 25;
            int maxLimit = 50; // Limite stricte

            if (rangeValues.Length >= 2)
            {
                if (!int.TryParse(rangeValues[0], out start)) start = 0;
                if (!int.TryParse(rangeValues[1], out end)) end = 25;
            }

            // Calcul et protection MaxLimit
            int countToFetch = end - start + 1;
            if (countToFetch > maxLimit)
            {
                countToFetch = maxLimit;
                end = start + countToFetch - 1;
            }

            // Compte total
            int totalItems = query.Count();

            // Exécution (Pagination)
            var entities = query.OrderBy(x => x.Id)
                                .Skip(start)
                                .Take(countToFetch)
                                .ToList();

            // Headers
            int finalEnd = Math.Min(end, totalItems > 0 ? totalItems - 1 : 0);
            
            // Content-Range: 0-25/48
            Response.Headers.Add("Content-Range", $"{start}-{finalEnd}/{totalItems}");
            // Accept-Range: product 50
            Response.Headers.Add("Accept-Range", $"{typeof(M).Name} {maxLimit}");
            
            // Link <...>; rel="next"
            GenerateLinkHeader(start, countToFetch, totalItems);

            return Ok(entities);
        }

        // Méthode Helper pour les liens (Next, Prev, First, Last)
        private void GenerateLinkHeader(int start, int limit, int totalItems)
        {
            var links = new List<string>();
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            // First
            links.Add($"<{baseUrl}?range=0-{limit - 1}>; rel=\"first\"");

            // Previous
            if (start > 0)
            {
                int prevStart = Math.Max(0, start - limit);
                int prevEnd = Math.Max(0, start - 1);
                links.Add($"<{baseUrl}?range={prevStart}-{prevEnd}>; rel=\"prev\"");
            }

            // Next
            if (start + limit < totalItems)
            {
                int nextStart = start + limit;
                int nextEnd = Math.Min(totalItems - 1, nextStart + limit - 1);
                links.Add($"<{baseUrl}?range={nextStart}-{nextEnd}>; rel=\"next\"");
            }

            // Last
            int lastStart = Math.Max(0, totalItems - limit);
            if (lastStart < 0) lastStart = 0;
            int lastEnd = Math.Max(0, totalItems - 1);
            links.Add($"<{baseUrl}?range={lastStart}-{lastEnd}>; rel=\"last\"");

            Response.Headers.Add("Link", string.Join(", ", links));
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