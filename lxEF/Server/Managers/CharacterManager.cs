using CitizenFX.Core;
using lxEF.Server.Data;
using lxEF.Server.Data.DTO;
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

        public async Task<Character> CreateCharacterAsync(CharacterDTO characterDTO, DBUser user, string ped = "")
        {
            Character character = null;
            try
            {
                character = new Character(characterDTO.FirstName, characterDTO.LastName, 18, characterDTO.DateOfBirth, characterDTO.Gender, characterDTO.Nationality, user, ped);
                Debug.WriteLine(character.FirstName);
                Debug.WriteLine(user.Username);
                if (character != null)
                {
                    using (var context = new lxDbContext())
                    {
                        Debug.WriteLine(user.Characters.Any().ToString());
                        user.Characters.Add(character);
                        _cachedCharacters.Add(character);
                        context.Entry(character).State = EntityState.Added;
                        int result = await context.SaveChangesAsync();
                        if (result > 0)
                        {
                            await ServerMain.LoadUsers();
                        }
                        Debug.WriteLine($"ROWS UPDATED {result}");
                    }

                    Debug.WriteLine($"Character {character.FirstName} {character.LastName} has been successfully created! OWNER: {user.Username}");
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
                Debug.WriteLine(username+ " " + citizenId);
                var character = await GetCharacterAsync(citizenId);
                Debug.WriteLine(character.FirstName+ " " + character.LastName);

                if (character != null)
                {
                    using (var context = new lxDbContext())
                    {
                        context.Characters.Remove(character);
                        _cachedCharacters.Remove(character);
                        int affectedRows = await context.SaveChangesAsync();
                        Debug.WriteLine($"Affected rows: {affectedRows}");
                    }

                    Debug.WriteLine($"Character {character.FirstName} {character.LastName} has been deleted! OWNER: {username}");
                    await ServerMain.LoadUsers();
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
                    characters = await context.Characters.Include(c => c.User).ToListAsync();
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
