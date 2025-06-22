﻿namespace SurveyBasket.Api.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Student, StudentResponse>()
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.MiddleName} {src.LastName}")
            .Map(dest => dest.Age, src => DateTime.Now.Year - src.DateOfBirth!.Value.Year,
                  srcCondition => srcCondition.DateOfBirth.HasValue)
           // .Map(dest => dest.DepartmentName, src => src.Department.Name)   this mapping will happen automatically because the name convention 
           .Ignore(dest => dest.DepartmentName);  // DepartementName will be ignored from mapping in the response 

        // mapping from student to studentResponse and vice versa
        // config.NewConfig<Student, StudentResponse>().TwoWays();



        // Question
        // first solution 
        config.NewConfig<QuestionRequest, Question>()
            .Map(dest => dest.Answers, src => src.Answers.Select(answer => new Answer { Content = answer }));

        // second solution
        //config.NewConfig<QuestionRequest, Question>()
        //    .Ignore(dest => dest.Answers);


        config.NewConfig<(ApplicationUser user, IList<string> roles), UserResponse>()
            .Map(dest => dest, src => src.user)
            .Map(dest => dest.Roles, src => src.roles);


        config.NewConfig<UpdateUserRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email);


    }
}
