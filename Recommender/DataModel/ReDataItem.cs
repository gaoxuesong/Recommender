using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.DataModel
{
    public class ReDataItem
    {
        public ReDataItem(int indexInGroup, string uniqueId)
        {
            this.UniqueId = uniqueId;
            this.IndexInGroup = indexInGroup;
        }

        public ReDataItem(int indexInGroup, string uniqueId, string title, string actor, string director, string imagePath, string description, string playurl)
        {
            this.IndexInGroup = indexInGroup;
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Actor = actor;
            this.Director = director;
            this.ImagePath = imagePath;
            this.Description = description;
            this.PlayUrl = playurl;
        }

        public ReDataItem(int indexInGroup, string uniqueId, string title, string actor, string director, string imagePath, string description, string playurl, string score)
        {
            this.IndexInGroup = indexInGroup;
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Actor = actor;
            this.Director = director;
            this.ImagePath = imagePath;
            this.Description = description;            
            this.PlayUrl = playurl;
            this.Score = score;
        }

        public int IndexInGroup { get; private set; }
        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Actor { get; private set; }
        public string Director { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string PlayUrl { get; private set; }
        public string Score { get; private set; }
        public override string ToString()
        {
            return this.Title;
        }
    }
}
