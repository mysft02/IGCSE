using BusinessObject.Model;
using BusinessObject.Payload.Request.Filter;
using System.Linq.Expressions;

namespace BusinessObject.DTOs.Request.Quizzes
{
    public class QuizQueryRequest : BaseQueryRequest
    {
        public int QuizId { get; set; }

        public int LessonId { get; set; }

        public override Expression<Func<T, bool>>? BuildFilter<T>() where T : class
        {
            if (typeof(T) != typeof(Quiz))
            {
                throw new NotSupportedException($"QuizQueryRequest only supports Quiz type, got {typeof(T).Name}");
            }

            return BuildQuizFilter() as Expression<Func<T, bool>>;
        }

        public List<Quiz> ApplySorting(List<Quiz> tests)
        {
            if (string.IsNullOrEmpty(SortBy))
                return tests;

            var isAscending = string.IsNullOrEmpty(SortOrder) || SortOrder.ToLower() == "asc";

            return SortBy.ToLower() switch
            {
                "quizid" => isAscending
                    ? tests.OrderBy(t => t.QuizId).ToList()
                    : tests.OrderByDescending(t => t.QuizId).ToList(),
                "lessonid" => isAscending
                    ? tests.OrderBy(t => t.LessonId).ToList()
                    : tests.OrderByDescending(t => t.LessonId).ToList(),
                "courseid" => isAscending
                    ? tests.OrderBy(t => t.CourseId).ToList()
                    : tests.OrderByDescending(t => t.CourseId).ToList(),
                "quiztitle" => isAscending
                    ? tests.OrderBy(t => t.QuizTitle).ToList()
                    : tests.OrderByDescending(t => t.QuizTitle).ToList(),
                "quizdescription" => isAscending
                    ? tests.OrderBy(t => t.QuizDescription).ToList()
                    : tests.OrderByDescending(t => t.QuizDescription).ToList(),
                "updatedat" => isAscending
                    ? tests.OrderBy(t => t.UpdatedAt).ToList()
                    : tests.OrderByDescending(t => t.UpdatedAt).ToList(),
                "createdat" => isAscending
                    ? tests.OrderBy(t => t.CreatedAt).ToList()
                    : tests.OrderByDescending(t => t.CreatedAt).ToList(),
                _ => tests
            };
        }

        public List<Quiz> ApplyPagination(List<Quiz> tests)
        {
            return tests
                .Skip(Page * GetPageSize())
                .Take(GetPageSize())
                .ToList();
        }

        private Expression<Func<Quiz, bool>>? BuildQuizFilter()
        {
            var predicates = new List<Expression<Func<Quiz, bool>>>();

            if (QuizId != null)
            {
                predicates.Add(x => x.QuizId == QuizId);
            }

            if (LessonId != null)
            {
                predicates.Add(x => x.LessonId == LessonId);
            }

            // Combine all predicates with AND
            if (!predicates.Any())
                return null;

            return predicates.Aggregate((expr1, expr2) =>
            {
                var parameter = Expression.Parameter(typeof(Quiz), "x");
                var body1 = Expression.Invoke(expr1, parameter);
                var body2 = Expression.Invoke(expr2, parameter);
                var combined = Expression.AndAlso(body1, body2);
                return Expression.Lambda<Func<Quiz, bool>>(combined, parameter);
            });
        }
    }
}
