using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lwt.Exceptions;
using Lwt.Interfaces;
using Lwt.Interfaces.Services;
using Lwt.Models;
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

        public Task CreateAsync(Guid userId, Text text)
        {
            text.UserId = userId;
            _textRepository.Add(text);
            return _transaction.Commit();
        }

        public Task<IEnumerable<Text>> GetByUserAsync(Guid userId)
        {
            return _textRepository.GetByUserAsync(userId);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            Text text = await _textRepository.GetByIdAsync(id);

            if (text.UserId == userId)
            {
                _textRepository.DeleteById(text);
                await _transaction.Commit();
            }
            else
            {
                throw new ForbiddenException("You don't have permission to delete this text");
            }
        }

        public Task EditAsync(Guid textId, Guid userId, TextEditModel editModel)
        {
            throw new NotImplementedException();
        }
    }
}