﻿ using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScorchApiV2.Interfaces;
using ScorchApiV2.ModelBinders;
using ScorchApiV2.Models;

namespace ScorchApiV2.Controllers
{
    [Route("api/[controller]")]
    public class CharacterController : Controller
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
        private static string characterTableName = "DnD-Characters";
        private static Table characterTable;

        public CharacterController()
        {
            characterTable = Table.LoadTable(client, characterTableName);
        }

        [HttpGet]
        public async Task<IList<Character>> Get()
        {
            var scanFilter = new ScanFilter();
            var search = characterTable.Scan(scanFilter);
            var characterList = new List<Character>();
            do
            {
                var documentList = await search.GetNextSetAsync();
                foreach (var document in documentList)
                {
                    var json = document.ToJson();
                    var ch = JsonConvert.DeserializeObject<Character>(json);
                    ch.OrganizeAbilities();
                    characterList.Add(ch);
                }
            } while (!search.IsDone);

            return characterList;
        }

        [HttpGet("{characterId}")]
        public async Task<Character> GetCharacter(Guid characterId)
        {
            var document = await characterTable.GetItemAsync(characterId);

            var ch = document != null ? JsonConvert.DeserializeObject<Character>(document.ToJson()) : null;
            ch?.OrganizeAbilities();

            return ch;
        }

        [HttpPost]
        public async Task<Character> PostCharacter([FromBody]Character character)
        {
            character.CharacterId = Guid.NewGuid();
            
            var document = Document.FromJson(JsonConvert.SerializeObject(character));

            await characterTable.PutItemAsync(document);

            return character;
        }

        [HttpPut("{characterId}")]
        public async Task<Character> PutCharacter(Guid characterId, [FromBody] Character character)
        {
            var document = Document.FromJson(JsonConvert.SerializeObject(character));
            document["CharacterId"] = characterId.ToString();

            await characterTable.PutItemAsync(document);

            return character;
        }

        [HttpPatch("{characterId}")]
        public async Task PatchCharacter(Guid characterId, [FromBody]Dictionary<string, string> props)
        {
            var document = new Document {["CharacterId"] = characterId.ToString()};
            foreach (var x in props)
            {
                document[x.Key] = x.Value;
            }

            await characterTable.UpdateItemAsync(document);
        }

        [HttpPost("{characterId}/inventory")]
        public async Task PostItemInInventory(Guid characterId, [FromBody, ModelBinder(BinderType = typeof(ItemModelBinder))] IItem item)
        {
            var itemController = new ItemController();
            var itemId = item.ItemId;
            // if no item id was passed in , assume it is a new item
            if (itemId == Guid.Empty)
            {
                item = await itemController.PostItem(item);
            }
            else
            {
                item = await itemController.GetItem(item.ItemId);
                if (item == null)
                {
                    throw new KeyNotFoundException("Item Id: " + itemId + " was not found");
                }
            }

            var character = await GetCharacter(characterId);
            character.Inventory.Add(item);
            var updateDocument = Document.FromJson(JsonConvert.SerializeObject(character));

            await characterTable.UpdateItemAsync(updateDocument);
        }

        [HttpPut("{characterId}/inventory")]
        public async Task PutItemInInventory(Guid characterId, [FromBody, ModelBinder(BinderType = typeof(ItemModelBinder))] IItem item)
        {
            var itemId = item.ItemId;
            // if no item id was passed in , assume it is a new item
            if (itemId == Guid.Empty)
            {
                throw new InvalidOperationException("Item does not contain an item id");
            }

            var character = await GetCharacter(characterId);
            var itemIndex = character.Inventory.FindIndex(x => x.ItemId == itemId);
            if (itemIndex == -1)
            {
                throw new InvalidOperationException("Character does not have this item");
            }
            character.Inventory[itemIndex] = item;

            // Also need to update the character equipment if they have that equipped
            character.UpdateEquipment(item);

            var updateDocument = Document.FromJson(JsonConvert.SerializeObject(character));
            await characterTable.UpdateItemAsync(updateDocument);
        }


        [HttpDelete("{characterId}")]
        public async Task DeleteCharacter(Guid characterId)
        {
            await characterTable.DeleteItemAsync(characterId);
        }

        [HttpDelete("{characterId}/inventory")]
        public async Task DeleteItemFromInventory(Guid characterId, Guid itemId)
        {
            var character = await GetCharacter(characterId);
            character.Inventory.RemoveAll(x => x.ItemId == itemId);

            var updateDocument = Document.FromJson(JsonConvert.SerializeObject(character));
            await characterTable.UpdateItemAsync(updateDocument);
        }

        [HttpPut("{characterId}/spells")]
        public async Task<Spell> PutSpell(Guid characterId, [FromBody] Spell spell)
        {
            // if no item id was passed in , assume it is a new item
            if (spell.SpellId == Guid.Empty)
            {
                var spellsController = new SpellsController();
                spell = await spellsController.PostSpell(spell);
            }

            var character = await GetCharacter(characterId);
            character.Spells.Add(spell);
            var updateDocument = Document.FromJson(JsonConvert.SerializeObject(character));

            await characterTable.UpdateItemAsync(updateDocument);

            return spell;
        }

        [HttpDelete("{characterId}/spells")]
        public async Task DeleteSpell(Guid characterId, Guid spellId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var character = await GetCharacter(characterId);
            character.Spells.RemoveAll(x => x.SpellId == spellId);

            var updateDocument = Document.FromJson(JsonConvert.SerializeObject(character));
            await characterTable.UpdateItemAsync(updateDocument);
        }

        [HttpPut("{characterId}/equipment")]
        public async Task<Equipment> PutCharacterEquipment(Guid characterId, [FromBody, ModelBinder(BinderType = typeof(ItemModelBinder))] IItem equipment)
        {
            var character = await GetCharacter(characterId);
            character.Equip(equipment);
            var doc = new Document
            {
                ["CharacterId"] = characterId,
                ["Equipment"] = Document.FromJson(JsonConvert.SerializeObject(character.Equipment))
            };

            await characterTable.UpdateItemAsync(doc);

            return character.Equipment;
        }

        [HttpDelete("{characterId}/equipment")]
        public async Task DeleteCharacterEquipment(Guid characterId, string slot)
        {
            var character = await GetCharacter(characterId);
            character.Unequip(slot);
            var doc = new Document
            {
                ["CharacterId"] = characterId,
                ["Equipment"] = Document.FromJson(JsonConvert.SerializeObject(character.Equipment))
            };

            await characterTable.UpdateItemAsync(doc);
        }
    }
}
