using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Nnn.ApplicationCore.Entities.Suggesstions;
using Microsoft.Nnn.ApplicationCore.Interfaces;
using Microsoft.Nnn.ApplicationCore.Services.SuggestionService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.SuggestionService
{
    public class SuggestionAppService:ISuggestionAppService
    {
        private readonly IAsyncRepository<Suggestion> _suggestionRepository;

        public SuggestionAppService(IAsyncRepository<Suggestion> suggestionRepository)
        {
            _suggestionRepository = suggestionRepository;
        }
        
        public async Task<Suggestion> Create(SuggestionCreate input)
        {
            var model = new Suggestion()
            {
                Content = input.Content,
                Email = input.Email
            };
            await _suggestionRepository.AddAsync(model);
            return model;
        }

        public async Task<List<Suggestion>> GetAll()
        {
            var result = await _suggestionRepository.GetAll().ToListAsync();
            return result;
        }
    }
}