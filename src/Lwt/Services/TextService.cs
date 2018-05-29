using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using LWT.Models;

namespace Lwt.Services
{
    public class TextService : ITextService
    {
        private readonly ITextRepository _textRepository;
        private readonly ITransaction _transaction;
        public TextService(ITextRepository textRepository, ITransaction transaction)
        {
            _textRepository = textRepository;
            _transaction = transaction;
        }
        public Task<bool> CreateAsync(Guid userId, Text text)
        {
            text.UserId = userId;
            _textRepository.Add(text);
            return _transaction.Commit();
        }

        public Task<IEnumerable<Text>> GetByUserAsync(Guid userId)
        {
            return _textRepository.GetByUserAsync(userId);
        }
    }
}
