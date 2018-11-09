using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace todolist_complete
{
    public class ChatMessageViewModel
    {
        public ObservableCollection<TodoItem> Messages { get; set; } = new ObservableCollection<TodoItem>();
    }
}
