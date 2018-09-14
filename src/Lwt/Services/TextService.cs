using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
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
        private readonly IValidator<Text> _textValidator;
        private readonly IMapper<TextEditModel, Text> _textEditMapper;

        public TextService(ITextRepository textRepository, ITransaction transaction,
            IMapper<TextEditModel, Text> textEditMapper, IValidator<Text> textValidator)
        {
            _textRepository = textRepository;
            _transaction = transaction;
            _textEditMapper = textEditMapper;
            _textValidator = textValidator;
        }

        public Task CreateAsync(Text text)
        {
            ValidationResult validationResult = _textValidator.Validate(text);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
            }

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