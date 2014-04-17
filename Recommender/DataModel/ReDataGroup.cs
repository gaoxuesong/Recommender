using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.DataModel
{
    public class ReDataGroup
    {
        public ReDataGroup(string uniqueId)
        {
            this.UniqueId = uniqueId;
            this.Items = new ObservableCollection<ReDataItem>();
        }

        public ReDataGroup(string uniqueId, string title)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Items = new ObservableCollection<ReDataItem>();
        }

        public ReDataGroup(string uniqueId, string title, string imagePath, string description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.ImagePath = imagePath;
            this.Description = description;
            this.Items = new ObservableCollection<ReDataItem>();
        }

        public ReDataGroup(string uniqueId, string title, string actor, string director, string imagePath, string description, string playurl)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Actor = actor;
            this.Director = director;
            this.ImagePath = imagePath;
            this.Description = description;
            this.PlayUrl = playurl;
            this.Items = new ObservableCollection<ReDataItem>();
        }

        public ReDataGroup(string uniqueId, string title, string actor, string director, string imagePath, string description, string playurl, string score)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Actor = actor;
            this.Director = director;
            this.ImagePath = imagePath;
            this.Description = description;            
            this.PlayUrl = playurl;
            this.Score = score;
            this.Items = new ObservableCollection<ReDataItem>();
        }

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

        public ObservableCollection<ReDataItem> Items { get; private set; }
    }
}
