<h1>EF Core GenericRepository</h1>

<p>This repository provides a generic way to handle common data access patterns using Entity Framework Core. Below is an example of how to use the repository with sorting, pagination, and including related entities.</p>

<h2>Usage Example</h2>

<pre><code>var sort1 = new Sort { Name = "Id", Type = SortTypes.ASC };
var sort2 = new Sort { Name = "Name", Type = SortTypes.DESC };
var pagination = new Pagination { Page = page, Count = count };

var list = await SchoolRepository.WhereAsync(
    filter: x => x.Classrooms.Any(y => y.Id == id),
    readOnly: true,
    includeLogicalDeleted: true,
    includes: school => new List&lt;object&gt; 
    { 
        school.Teachers, 
        school.Classrooms.Select(classroom => new List&lt;object&gt; 
        { 
            classroom.Students, 
            classroom.Desks 
        }) 
    },
    pagination: pagination,
    sorts: new[] { sort1, sort2 }
);
</code></pre>

<p>This code retrieves a list of schools with the following features:</p>
<ul>
  <li>Filters by the <code>Id</code> property.</li>
  <li>Applies sorting by <code>Id</code> in ascending order and <code>Name</code> in descending order.</li>
  <li>Paginates the result set using the <code>Page</code> and <code>Count</code> values.</li>
  <li>Includes related entities like <code>Teachers</code>, <code>Classrooms</code>, <code>Students</code>, and <code>Desks</code>.</li>
  <li>Optionally retrieves logically deleted entities if required.</li>
</ul>

<h2>Equivalent EF Core Query</h2>

<pre><code>var list = await DbContext.Schools
    .AsNoTracking()
    .Include(school => school.Teachers)
    .Include(school => school.Classrooms).ThenInclude(classroom => classroom.Students)
    .Include(school => school.Classrooms).ThenInclude(classroom => classroom.Desks)
    .Where(school => school.Id == id)
    .OrderBy(school => school.Classrooms.Any(classroom => classroom.Id == id))
    .ThenByDescending(school => school.Name)
    .Skip(page * count)
    .Take(count)
    .ToListAsync();
</code></pre>

<p>This query:</p>
<ul>
  <li>Disables entity tracking (<code>AsNoTracking</code>) for read-only operations.</li>
  <li>Includes <code>Teachers</code>, <code>Classrooms</code>, <code>Students</code>, and <code>Desks</code> using multiple <code>Include</code> and <code>ThenInclude</code> statements.</li>
  <li>Filters, sorts, and paginates the results.</li>
</ul>
