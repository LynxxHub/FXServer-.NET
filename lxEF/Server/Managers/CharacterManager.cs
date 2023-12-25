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
        private readonly DBUserManager _dbUserManager;
        private readonly object _lockObject = new object();
        private List<Character> _cachedCharacters;

        public CharacterManager(DBUserManager dbUserManager)
        {
            _cachedCharacters = new List<Character>();
            _dbUserManager = dbUserManager;

            Task.Run(async () =>
            {
                _cachedCharacters = await GetAllCharactersAsync();
            });

            DatabaseSyncTask();
        }

        public async Task<Character> CreateCharacterAsync(CharacterDTO characterDTO, DBUser user, string ped = "")
        {
            Character character = null;
            try
            {
                character = new Character(characterDTO.FirstName, characterDTO.LastName, 18, characterDTO.DateOfBirth, characterDTO.Gender, characterDTO.Nationality, user, ped);
                if (character != null)
                {
                    using (var context = new lxDbContext())
                    {
                        _cachedCharacters.Add(character);
                        context.Entry(character).State = EntityState.Added;
                        int result = await context.SaveChangesAsync();
                        if (result > 0)
                        {
                            _dbUserManager.SyncAllUsers();
                            await _dbUserManager.LoadUsersAsync();
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
                    _dbUserManager.SyncAllUsers();
                    await _dbUserManager.LoadUsersAsync();

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

        //Caching mechanism
        //TODO: Change into a service
        private void SyncCharacter(Character cachedChar)
        {
            lock (_lockObject)
            {
                using (var context = new lxDbContext())
                {
                    var dbChar = context.Characters.FirstOrDefault(c => c.CharacterID == c.CharacterID);
                    context.Entry(dbChar).CurrentValues.SetValues(cachedChar);
                    context.SaveChanges();
                }
            }
        }

        public void SyncAllCharacters()
        {
            lock (_lockObject)
            {
                using (var context = new lxDbContext())
                {
                    foreach (var cachedChar in _cachedCharacters)
                    {
                        var dbChar = context.Characters.FirstOrDefault(c => c.CharacterID == c.CharacterID);
                        if (dbChar != null)
                        {
                            context.Entry(dbChar).CurrentValues.SetValues(cachedChar);
                        }
                    }

                    context.SaveChanges();
                }
            }
        }

        private void DatabaseSyncTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    //make configurable
                    await Task.Delay(5 * 60 * 1000);
                    SyncAllCharacters();
                }
            });
        }
    }
}
