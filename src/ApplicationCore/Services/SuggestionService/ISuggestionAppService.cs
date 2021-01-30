using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Nnn.ApplicationCore.Entities.Suggesstions;
using Microsoft.Nnn.ApplicationCore.Services.SuggestionService.Dto;

namespace Microsoft.Nnn.ApplicationCore.Services.SuggestionService
{
    public interface ISuggestionAppService
    {
        Task<Suggestion> Create(SuggestionCreate input);
        Task<List<Suggestion>> GetAll();
    }
}