using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Archi.Library.Models;
using Archi.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Archi.Library.Controllers
{
    [ApiController]
    public abstract class BaseController<C, M> : ControllerBase
        where C : BaseDbContext
        where M : BaseModel
    {
        protected readonly C _context;

        protected BaseController(C context) => _context = context;

        // GET ALL (pagination)
        [HttpGet]
        public virtual ActionResult<IEnumerable<M>> GetAll([FromQuery] string range = "0-25")
        {
            var query = _context.Set<M>().Where(x => !x.IsDeleted);
            var rangeValues = range.Split('-');
            int start = 0, end = 25, maxLimit = 50;

            if (rangeValues.Length >= 2)
            {
                if (int.TryParse(rangeValues[0], out int s)) start = s;
                if (int.TryParse(rangeValues[1], out int e)) end = e;
            }

            int countToFetch = Math.Min(maxLimit, end - start + 1);
            end = start + countToFetch - 1;

            int totalItems = query.Count();
            var entities = query.OrderBy(x => x.Id).Skip(start).Take(countToFetch).ToList();

            int finalEnd = Math.Min(end, totalItems > 0 ? totalItems - 1 : 0);
            Response.Headers.Add("Content-Range", $"{start}-{finalEnd}/{totalItems}");
            Response.Headers.Add("Accept-Range", $"{typeof(M).Name} {maxLimit}");
            GenerateLinkHeader(start, countToFetch, totalItems);

            return Ok(entities);
        }

        private void GenerateLinkHeader(int start, int limit, int totalItems)
        {
            var links = new List<string>();
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            links.Add($"<{baseUrl}?range=0-{limit - 1}>; rel=\"first\"");

            if (start > 0)
            {
                int prevStart = Math.Max(0, start - limit);
                int prevEnd = Math.Max(0, start - 1);
                links.Add($"<{baseUrl}?range={prevStart}-{prevEnd}>; rel=\"prev\"");
            }

            if (start + limit < totalItems)
            {
                int nextStart = start + limit;
                int nextEnd = Math.Min(totalItems - 1, nextStart + limit - 1);
                links.Add($"<{baseUrl}?range={nextStart}-{nextEnd}>; rel=\"next\"");
            }

            int lastStart = Math.Max(0, totalItems - limit);
            int lastEnd = Math.Max(0, totalItems - 1);
            links.Add($"<{baseUrl}?range={lastStart}-{lastEnd}>; rel=\"last\"");

            Response.Headers.Add("Link", string.Join(", ", links));
        }

        // SEARCH + FILTRES + TRI + PAGINATION + FIELDS
        [HttpGet("search")]
        public virtual ActionResult<IEnumerable<M>> Search([FromQuery] ResourceQueryParams q)
        {
            var query = _context.Set<M>().Where(x => !x.IsDeleted);

            query = ApplyFilters(query, q);

           
            query = ApplySort(query, q.Sort, q.Asc, q.Desc);

            var rangeValues = q.Range.Split('-');
            int start = 0, end = 25, maxLimit = 50;

            if (rangeValues.Length >= 2)
            {
                if (int.TryParse(rangeValues[0], out int s)) start = s;
                if (int.TryParse(rangeValues[1], out int e)) end = e;
            }

            int countToFetch = Math.Min(maxLimit, end - start + 1);
            end = start + countToFetch - 1;

            int totalItems = query.Count();

            var entities = query.Skip(start).Take(countToFetch).ToList();

            int finalEnd = Math.Min(end, totalItems > 0 ? totalItems - 1 : 0);
            Response.Headers.Add("Content-Range", $"{start}-{finalEnd}/{totalItems}");
            Response.Headers.Add("Accept-Range", $"{typeof(M).Name} {maxLimit}");
            GenerateLinkHeader(start, countToFetch, totalItems);

            if (!string.IsNullOrWhiteSpace(q.Fields))
            {
                var partial = ApplyPartialFields(entities, q.Fields);
                return Ok(partial);
            }

            return Ok(entities);
        }

        private IQueryable<M> ApplyFilters(IQueryable<M> query, ResourceQueryParams p)
        {
            if (!string.IsNullOrWhiteSpace(p.Type))
            {
                var values = p.Type.Split(',')
                                   .Select(v => v.Trim())
                                   .Where(v => !string.IsNullOrEmpty(v));

                var typeProp = typeof(M).GetProperty("Type");
                if (typeProp != null)
                {
                    var param = Expression.Parameter(typeof(M), "x");
                    var body = Expression.Property(param, typeProp);

                    Expression? predicate = null;
                    foreach (var value in values)
                    {
                        var constant = Expression.Constant(value, typeof(string));
                        var equal = Expression.Equal(body, constant);
                        predicate = predicate == null ? equal : Expression.Or(predicate, equal);
                    }

                    if (predicate != null)
                    {
                        var lambda = Expression.Lambda<Func<M, bool>>(predicate, param);
                        query = query.Where(lambda);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(p.Name))
            {
                var searchText = p.Name.Trim('*').ToLower();
                var nameProp = typeof(M).GetProperty("Name");
                if (nameProp != null)
                {
                    var param = Expression.Parameter(typeof(M), "x");
                    var body = Expression.Property(param, nameProp);
                    var toLower = Expression.Call(body, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
                    var contains = Expression.Call(
                        toLower,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) })!,
                        Expression.Constant(searchText)
                    );
                    var lambda = Expression.Lambda<Func<M, bool>>(contains, param);
                    query = query.Where(lambda);
                }
            }

            foreach (var prop in typeof(M).GetProperties())
            {
                if (new[] { "Rating", "Price" }.Contains(prop.Name))
                {
                    string? valueStr = prop.Name switch
                    {
                        "Rating" => p.Rating,
                        "Price"  => p.Price,
                        _        => null
                    };

                    if (!string.IsNullOrWhiteSpace(valueStr))
                        query = ApplyNumericFilter(query, prop, valueStr);
                }
            }

            if (!string.IsNullOrWhiteSpace(p.CreatedAt))
            {
                var createdProp = typeof(M).GetProperty("CreatedAt");
                if (createdProp != null)
                    query = ApplyDateFilter(query, createdProp, p.CreatedAt);
            }

            return query;
        }

        private IQueryable<M> ApplyNumericFilter(IQueryable<M> query, PropertyInfo property, string rangeStr)
        {
            if (!rangeStr.StartsWith("["))
            {
                var param = Expression.Parameter(typeof(M), "x");
                var body  = Expression.Property(param, property);

                Expression? predicate = null;
                foreach (var s in rangeStr.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)))
                {
                    var val      = Convert.ChangeType(s, property.PropertyType);
                    var constant = Expression.Constant(val, property.PropertyType);
                    var equal    = Expression.Equal(body, constant);
                    predicate    = predicate == null ? equal : Expression.Or(predicate, equal);
                }

                if (predicate != null)
                {
                    var lambda = Expression.Lambda<Func<M, bool>>(predicate, param);
                    query = query.Where(lambda);
                }

                return query;
            }

            var cleaned = rangeStr.Trim('[', ']');
            var parts   = cleaned.Split(',');

            var p2   = Expression.Parameter(typeof(M), "x");
            var prop2 = Expression.Property(p2, property);

            Expression? minExpr = null;
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                var min = Convert.ChangeType(parts[0].Trim(), property.PropertyType);
                minExpr = Expression.GreaterThanOrEqual(prop2, Expression.Constant(min, property.PropertyType));
            }

            Expression? maxExpr = null;
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                var max = Convert.ChangeType(parts[1].Trim(), property.PropertyType);
                maxExpr = Expression.LessThanOrEqual(prop2, Expression.Constant(max, property.PropertyType));
            }

            Expression? combined = null;
            if (minExpr != null) combined = minExpr;
            if (maxExpr != null) combined  = combined == null ? maxExpr : Expression.And(combined, maxExpr);

            if (combined != null)
            {
                var lambda = Expression.Lambda<Func<M, bool>>(combined, p2);
                query = query.Where(lambda);
            }

            return query;
        }

        private IQueryable<M> ApplyDateFilter(IQueryable<M> query, PropertyInfo property, string rangeStr)
        {
            if (!rangeStr.StartsWith("["))
            {
                var param = Expression.Parameter(typeof(M), "x");
                var body  = Expression.Property(param, property);

                Expression? predicate = null;
                foreach (var s in rangeStr.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)))
                {
                    var val      = DateTime.Parse(s);
                    var constant = Expression.Constant(val);
                    var equal    = Expression.Equal(body, constant);
                    predicate    = predicate == null ? equal : Expression.Or(predicate, equal);
                }

                if (predicate != null)
                {
                    var lambda = Expression.Lambda<Func<M, bool>>(predicate, param);
                    query = query.Where(lambda);
                }

                return query;
            }

            var cleaned = rangeStr.Trim('[', ']');
            var parts   = cleaned.Split(',');

            var p2    = Expression.Parameter(typeof(M), "x");
            var prop2 = Expression.Property(p2, property);

            Expression? minExpr = null;
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                var min = DateTime.Parse(parts[0].Trim());
                minExpr = Expression.GreaterThanOrEqual(prop2, Expression.Constant(min));
            }

            Expression? maxExpr = null;
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                var max = DateTime.Parse(parts[1].Trim());
                maxExpr = Expression.LessThanOrEqual(prop2, Expression.Constant(max));
            }

            Expression? combined = null;
            if (minExpr != null) combined = minExpr;
            if (maxExpr != null) combined  = combined == null ? maxExpr : Expression.And(combined, maxExpr);

            if (combined != null)
            {
                var lambda = Expression.Lambda<Func<M, bool>>(combined, p2);
                query = query.Where(lambda);
            }

            return query;
        }

        private IQueryable<M> ApplySort(IQueryable<M> query, string? sort, string? asc, string? desc)
        {
            var sortFields = sort?.Split(',').Select(s => s.Trim()) ?? Array.Empty<string>();
            var descFields = desc?.Split(',').Select(s => s.Trim()) ?? Array.Empty<string>();
            var ascFields  = asc?.Split(',').Select(s => s.Trim())  ?? Array.Empty<string>();

            var allFields = sortFields
                .Union(ascFields)
                .Union(descFields)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct();

            IOrderedQueryable<M>? ordered = null;

            foreach (var field in allFields)
            {
                var property = typeof(M).GetProperty(
                    field,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                );
                if (property == null) continue;

                var param = Expression.Parameter(typeof(M), "x");
                var body  = Expression.Property(param, property);

                var converted = Expression.Convert(body, typeof(object));
                var lambda    = Expression.Lambda<Func<M, object>>(converted, param);

                bool isDesc = descFields.Contains(field, StringComparer.OrdinalIgnoreCase);

                if (isDesc)
                    ordered = ordered == null ? query.OrderByDescending(lambda) : ordered.ThenByDescending(lambda);
                else
                    ordered = ordered == null ? query.OrderBy(lambda) : ordered.ThenBy(lambda);
            }

            return ordered ?? query;
        }

        private List<object> ApplyPartialFields(List<M> entities, string fields)
        {
            var fieldNames = fields.Split(',')
                                   .Select(f => f.Trim())
                                   .Where(f => !string.IsNullOrEmpty(f));

            var result = new List<object>();

            foreach (var entity in entities)
            {
                var obj = (IDictionary<string, object?>)new ExpandoObject();
                foreach (var field in fieldNames)
                {
                    var prop = typeof(M).GetProperty(
                        field,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                    );
                    if (prop != null)
                        obj[field] = prop.GetValue(entity);
                }
                result.Add(obj);
            }

            return result;
        }

        // GET BY ID
        [HttpGet("{id}")]
        public virtual ActionResult<M> GetById(int id)
        {
            var entity = _context.Set<M>().Find(id);
            return (entity == null || entity.IsDeleted) ? NotFound() : Ok(entity);
        }

        // POST
        [HttpPost]
        public virtual ActionResult<M> Post([FromBody] M entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Set<M>().Add(entity);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        // PUT
        [HttpPut("{id}")]
        public virtual ActionResult Put(int id, [FromBody] M entity)
        {
            if (id != entity.Id)
                return BadRequest();

            var existing = _context.Set<M>().Find(id);
            if (existing == null || existing.IsDeleted)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(existing).CurrentValues.SetValues(entity);
            _context.SaveChanges();

            return NoContent();
        }

        // DELETE (soft delete)
        [HttpDelete("{id}")]
        public virtual ActionResult Delete([FromRoute] int id)
        {
            var entity = _context.Set<M>().Find(id);
            if (entity == null || entity.IsDeleted)
                return NotFound();

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return NoContent();
        }
    }
}