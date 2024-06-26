﻿using Fakebook.Application.Generics;
using Fakebook.Application.Profile.Commands;
using Fakebook.DAL;
using FakeBook.Domain.Aggregates.UserProfileAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Fakebook.Application.Profile.CommandHandlers
{
    public class PostUserProfileCmdHanlder(DataContext context) : IRequestHandler<PostUserProfileCmd, Response<UserProfile>>
    {
        private readonly DataContext _context = context;

        public async Task<Response<UserProfile>> Handle(PostUserProfileCmd request, CancellationToken cancellationToken)
        {
            UserProfile? existed = null;
            var response = new Response<UserProfile>();

            if (request.UserProfileId != default) // passed ? get its profile
            {
                existed = await _context.Set<UserProfile>().FindAsync(request.UserProfileId, cancellationToken);

                if (existed is null)
                {
                        response.Success = false;
                        response.Errors.Add(new ErrorResult { Status = Generics.Enums.StatusCode.NotFound, Message = "UserProfile is not exist" });
                    return response;
                       }
            }

            var info = GeneralInfo.CreateBasicInfo(
                request.FirstName,
                request.LastName,
                request.EmailAddress,
                request.Phone,
                request.DateOfBirth,
                request.City
            );

            if (existed is null) // creation case
            {
                var newUserProfile = UserProfile.CreateUserProfile(Guid.NewGuid().ToString(), info);
                await _context.Set<UserProfile>().AddAsync(newUserProfile, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else // update
            {
                existed.UpdateBasicInfo(info);
                _context.Set<UserProfile>().Update(existed);
                await _context.SaveChangesAsync(cancellationToken);
                response.Payload = existed;
            }
                return response;
        }

    }
}
