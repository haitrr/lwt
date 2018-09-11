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
        private readonly IMapper<TextEditModel, Text> _textEditMapper;

        public TextService(ITextRepository textRepository, ITransaction transaction,
            IMapper<TextEditModel, Text> textEditMapper)
        {
            _textRepository = textRepository;
            _transaction = transaction;
            _textEditMapper = textEditMapper;
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

        public async Task EditAsync(Guid textId, Guid userId, TextEditModel editModel)
        {
            Text text = await _textRepository.GetByIdAsync(textId);

            if (text.UserId == userId)
            {
                Text editedText = _textEditMapper.Map(editModel, text);
                _textRepository.Update(editedText);
                await _transaction.Commit();
            }
            else
            {
                throw new ForbiddenException("You do not have permission to edit this text");
            }
        }
    }
}