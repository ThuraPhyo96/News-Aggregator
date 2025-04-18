﻿using NewsAggregator.Application.Common;
using NewsAggregator.Application.DTOs;
using NewsAggregator.Application.Helpers;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Mappers;
using NewsAggregator.Domain.Interfaces;

namespace NewsAggregator.Application.Services
{
    public class UserAppService : IUserAppService
    {
        private readonly IUserRepository _userRepository;

        public UserAppService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<UserDto>> GetByUsername(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return Result<UserDto>.Fail("User name can not be null or empty.");

                if (!IdValidationHelper.IsValidHexadecimalId(username))
                    return Result<UserDto>.Fail("Invalid user format.");

                var obj = await _userRepository.GetByUsernameAsync(username);
                if (obj is null)
                    return Result<UserDto>.Fail("Not found!");

                return Result<UserDto>.Ok(UserMapper.ToDto(obj!)!);
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<UserDto>> CreateUser(CreateUserDto? input)
        {
            try
            {
                if (input is null)
                    return Result<UserDto>.Fail("User can not be null.");

                if (string.IsNullOrWhiteSpace(input.Username) ||
                    string.IsNullOrWhiteSpace(input.Password))
                {
                    return Result<UserDto>.Fail("Username and Password cannot be empty or whitespace.");
                }

                var user = UserMapper.ToEntity(input);
                if (user is null)
                    return Result<UserDto>.Fail("Invalid user data.");

                var returnUser = await _userRepository.AddAsync(user);
                if (returnUser is null)
                    return Result<UserDto>.Fail("Failed to save user.");

                var dto = UserMapper.ToDto(returnUser);
                return Result<UserDto>.Ok(dto!);
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Fail($"An error occurred: {ex.Message}");
            }
        }

        public async Task<Result<string>> GetToken(LoginUserDto input)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(input.Username!);
                if (user == null || _userRepository.VerifyUser(input.Password!, user.PasswordHash!))
                    return Result<string>.Fail($"Invalid credentials.");

                var token = _userRepository.GetToken(user.Username!);
                return Result<string>.Ok(token);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"An error occurred: {ex.Message}");
            }
        }
    }
}
