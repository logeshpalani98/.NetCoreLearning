﻿using System;
using System.Collections.Generic;
using System.Linq;
using UserDataFlow.Interface;
using UserDataFlow.Model.Address;
using UserDataFlow.Models;

namespace UserDataFlow.Repository
{
    public class AddressRepository : IAddress
    {
        private readonly logesh_user_task_dbContext _dbContext;

        public AddressRepository(logesh_user_task_dbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<AddressRes> GetAddress(int userId)
        {
            var address = (from userAddress in _dbContext.UserAddress
                                        from baseAddress in _dbContext.BaseAddress
                                        where baseAddress.PinCode == userAddress.PinCode
                                        where userAddress.UserId == userId
                                        select new AddressRes()
                                        {
                                            UserId = userAddress.UserId,
                                            DoorNo = userAddress.DoorNo,
                                            PinCode = userAddress.PinCode,
                                            AddressId = userAddress.AddressId,
                                            City = baseAddress.City,
                                            District = baseAddress.District,
                                            Nationality = baseAddress.Nationality,
                                            Street = userAddress.Street
                                        }).ToList();

            if (address.Any())
                return address;

            throw new Exception("Address not exist");
        }

        public AddressRes GetAddress(int userId, int addressId)
        {
            var userAddress = _dbContext.UserAddress.FirstOrDefault(c => c.UserId == userId && c.AddressId == addressId);

            if (userAddress!=null)
            {
                var baseAddress = _dbContext.BaseAddress.FirstOrDefault(c => c.PinCode == userAddress.PinCode);
                if(baseAddress != null)
                    return new AddressRes()
                    {
                        UserId = userAddress.UserId,
                        DoorNo = userAddress.DoorNo,
                        PinCode = userAddress.PinCode,
                        AddressId = userAddress.AddressId,
                        City = baseAddress.City,
                        District = baseAddress.District,
                        Nationality = baseAddress.Nationality,
                        Street = userAddress.Street
                    };
            }

            throw new Exception("Address not exist");
        }

        public AddressRes AddAddress( AddressReq req)
        {
            if (_dbContext.User.FirstOrDefault(c => c.UserId == req.UserId) != null)
            {
                if (!string.IsNullOrEmpty(req.PinCode.Trim())&& 
                    !string.IsNullOrEmpty(req.City.Trim()) && 
                    !string.IsNullOrEmpty(req.District.Trim())&&
                    !string.IsNullOrEmpty(req.Nationality.Trim()))
                {
                        if (!_dbContext.BaseAddress.Any(c => c.PinCode == req.PinCode))
                        {
                            var baseAddress = new BaseAddress()
                            {
                                PinCode = req.PinCode,
                                City = req.City,
                                District = req.District,
                                Nationality = req.Nationality
                            };

                            _dbContext.BaseAddress.Add(baseAddress);
                            _dbContext.SaveChanges();
                        }

                        var userAddress = new UserAddress()
                        {
                            UserId = req.UserId,
                            DoorNo = req.DoorNo??"",
                            Street = req.Street??"",
                            PinCode = req.PinCode
                        };

                        _dbContext.UserAddress.Add(userAddress);
                        _dbContext.SaveChanges();

                        return new AddressRes()
                        {
                            UserId = req.UserId,
                            DoorNo = req.DoorNo,
                            PinCode = req.PinCode,
                            AddressId = userAddress.AddressId,
                            City = req.City,
                            District = req.District,
                            Nationality = req.Nationality,
                            Street = req.Street
                        };
                }
                throw new ArgumentException("Invalid address, please enter correct address");
            }
            throw new ArgumentException("User not exists");
        }

        public AddressRes UpdateAddress(AddressRes req)
        {
            var differAddress = _dbContext.UserAddress.FirstOrDefault(c => c.UserId == req.UserId && c.AddressId == req.AddressId);

            if (differAddress != null)
            {
                if (!string.IsNullOrEmpty(req.PinCode.Trim()) &&
                    !string.IsNullOrEmpty(req.City.Trim()) &&
                    !string.IsNullOrEmpty(req.District.Trim()) &&
                    !string.IsNullOrEmpty(req.Nationality.Trim()))
                {
                    if (differAddress.PinCode != req.PinCode)
                    {
                        if (_dbContext.UserAddress.Where(c => c.PinCode == differAddress.PinCode).ToList().Count() <= 1)
                        {
                            var baseAddress = _dbContext.BaseAddress.FirstOrDefault(c => c.PinCode == differAddress.PinCode);
                            
                            if (baseAddress != null)
                            {
                                baseAddress.City = req.City;
                                baseAddress.District = req.District;
                                baseAddress.Nationality = req.Nationality;

                                _dbContext.Update(baseAddress);
                            }

                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            if (_dbContext.BaseAddress.Any(c => c.PinCode == req.PinCode))
                                differAddress.PinCode = req.PinCode;
                            else
                            {
                                var baseAddress = new BaseAddress()
                                {
                                    City = req.City,
                                    District = req.District,
                                    Nationality = req.City,
                                    PinCode = req.PinCode
                                };
                                _dbContext.BaseAddress.Add(baseAddress);
                                _dbContext.SaveChanges();

                                differAddress.PinCode = req.PinCode;
                            }
                        }
                    }

                    differAddress.DoorNo = req.DoorNo;
                    differAddress.Street = req.Street;
                    _dbContext.UserAddress.Update(differAddress);
                    _dbContext.SaveChanges();

                    return req;
                }

                throw new Exception("Invalid address, please enter correct address");
            }
            
            throw new Exception("Address not exist");
        }

        public bool DeleteAddress(int userId, int addressId)
        {
            var address = _dbContext.UserAddress.FirstOrDefault(c=>c.UserId == userId && c.AddressId == addressId);
            
            if (address!=null)
            {
                _dbContext.UserAddress.Remove(address);

                if (_dbContext.UserAddress.Where(c => c.PinCode == address.PinCode).ToList().Count()<2)
                {
                    var baseAddress=_dbContext.BaseAddress.FirstOrDefault(c => c.PinCode == address.PinCode);
                    if (baseAddress!=null) _dbContext.Remove(baseAddress);
                }

                _dbContext.SaveChanges();

                return true;
            }

            throw new Exception("Contact not exist");
        }
    }
}