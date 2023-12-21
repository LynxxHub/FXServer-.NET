using CitizenFX.Core;
using lxEF.Server.Data;
using lxEF.Server.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lxEF.Server.Managers
{
    public class CharacterManager
    {
        private List<Character> _cachedCharacters;

        public CharacterManager()
        {
            _cachedCharacters = new List<Character>();

            Task.Run(async () =>
            {
                _cachedCharacters = await GetAllCharactersAsync();
            });
        }

        public async Task<Character> CreateCharacterAsync(string firstName, string lastName, int age, DateTime dob, string gender, string nationality, DBUser user, string ped = "")
        {
            Character character = null;
            try
            {
                character = new Character(firstName, lastName, age, dob, gender, nationality, user, ped);

                if (character != null)
                {
                    using (var context = new lxDbContext())
                    {
                        context.Characters.Add(character);
                        _cachedCharacters.Add(character);
                        await context.SaveChangesAsync();
                    }

                    Debug.WriteLine($"Character {firstName} {lastName} has been created successfully! OWNER: {user.Username}");

                }


                return character;
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return character;
            }
        }

        public async Task<bool> RemoveCharacterAsync(string citizenId, string username)
        {
            try
            {
                var character = GetCharacter(citizenId);

                if (character != null)
                {
                    using (var context = new lxDbContext())
                    {
                        context.Characters.Remove(character);
                        _cachedCharacters.Remove(character);
                        await context.SaveChangesAsync();
                    }

                    Debug.WriteLine($"Character {character.FirstName} {character.LastName} has been deleted! OWNER: {username}");
                    return true;
                }


                return false;
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return false;
            }

        }

        public Character GetCharacter(string citizenId)
        {
            return _cachedCharacters.FirstOrDefault(c => c.CitizenID == citizenId);
        }

        public async Task<Character> GetCharacterAsync(string citizenId)
        {
            Character character = null;
            try
            {
                using (var context = new lxDbContext())
                {
                    character = await context.Characters.FirstOrDefaultAsync(c => c.CitizenID == citizenId);
                    return character;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return character;
            }
        }

        public List<Character> GetAllCharacters()
        {
            return _cachedCharacters;
        }

        public async Task<List<Character>> GetAllCharactersAsync()
        {
            List<Character> characters = new List<Character>();
            try
            {
                using (var context = new lxDbContext())
                {
                    characters = await context.Characters.ToListAsync();
                    return characters;
                }
            }
            catch (Exception ex)
            {
                LoggingManager.PrintExceptions(ex);
                return characters;
            }
        }
    }
}
