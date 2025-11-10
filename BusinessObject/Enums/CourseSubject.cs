namespace BusinessObject.Enums
{
    public enum CourseSubject
    {
        Mathematics_0580,
        Mathematics_US_0444,
        Mathematics_9_1_0980,
        Mathematics_International_0607,
        Mathematics_Additional_0606
    }

    public static class CourseSubjectHelper
    {
        // 👉 Chuyển enum → chuỗi hiển thị
        public static string GetDisplayName(CourseSubject subject)
        {
            return subject switch
            {
                CourseSubject.Mathematics_0580 => "Mathematics - 0580",
                CourseSubject.Mathematics_US_0444 => "Mathematics (US) - 0444",
                CourseSubject.Mathematics_9_1_0980 => "Mathematics (9-1) - 0980",
                CourseSubject.Mathematics_International_0607 => "Mathematics - International - 0607",
                CourseSubject.Mathematics_Additional_0606 => "Mathematics - Additional - 0606",
                _ => subject.ToString()
            };
        }

        // 👉 Chuyển chuỗi hiển thị → enum
        public static CourseSubject? ParseDisplayName(string name)
        {
            return name switch
            {
                "Mathematics - 0580" => CourseSubject.Mathematics_0580,
                "Mathematics (US) - 0444" => CourseSubject.Mathematics_US_0444,
                "Mathematics (9-1) - 0980" => CourseSubject.Mathematics_9_1_0980,
                "Mathematics - International - 0607" => CourseSubject.Mathematics_International_0607,
                "Mathematics - Additional - 0606" => CourseSubject.Mathematics_Additional_0606,
                _ => null
            };
        }
    }
}
