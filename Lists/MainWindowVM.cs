using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Lists
{
    internal sealed class MainWindowVM
    {
        private static bool _isDirty;

        internal sealed class Item
        {
            public Item(string text = "Text") => Text = text;

            private string _text = null!;
            public string Text
            {
                get => _text;
                set
                {
                    _text = value;
                    _isDirty = true;
                }
            }
        }

        private readonly Serializer _serializer;

        private bool? _lastSelectedItem;

        public MainWindowVM()
        {
            _serializer = new Serializer();

            Task.Run(async () => await InitializeLists());

            MoveToSecond = new RelayCommand(execute =>
            {
                SecondList.Add(FirstSelectedItem!);
                FirstList.Remove(FirstSelectedItem!);
                _lastSelectedItem = SecondSelectedItem != null ? true : null;
                _isDirty = true;
            }, canExecute => FirstSelectedItem != null);

            MoveToFirst = new RelayCommand(execute =>
            {
                FirstList.Add(SecondSelectedItem!);
                SecondList.Remove(SecondSelectedItem!);
                _lastSelectedItem = FirstSelectedItem != null ? true : null;
                _isDirty = true;
            }, canExecute => SecondSelectedItem != null);

            AddCommand = new RelayCommand(execute =>
            {
                if (_lastSelectedItem == false)
                    FirstList.Add(new Item());
                else
                    SecondList.Add(new Item());
                _isDirty = true;
            }, canExecute => HasSelectedItem);

            DeleteCommand = new RelayCommand(execute =>
            {
                if (_lastSelectedItem == false)
                {
                    FirstList.Remove(FirstSelectedItem!);
                    _lastSelectedItem = SecondSelectedItem != null ? true : null;
                }
                else
                {
                    SecondList.Remove(SecondSelectedItem!);
                    _lastSelectedItem = FirstSelectedItem != null ? true : null;
                }
                _isDirty = true;
            }, canExecute => HasSelectedItem);

            SaveCommand = new RelayCommand(async execute =>
            {
                StringBuilder text = new();
                foreach (Item item in FirstList)
                    text.Append($"{item.Text}\t");
                text.AppendLine();
                foreach (Item item in SecondList)
                    text.Append($"{item.Text}\t");

                await _serializer.Serialize(text.ToString());
                _isDirty = false;
            }, canExecute => _isDirty);

            MoveDown = new RelayCommand(execute =>
            {
                if (_lastSelectedItem == false)
                {
                    if (_fIndex < FirstList.Count - 1)
                    {
                        FirstList.Move((int)_fIndex!, (int)(++_fIndex));
                        _isDirty = true;
                    }
                }
                else
                {
                    if (_sIndex < SecondList.Count - 1)
                    {
                        SecondList.Move((int)_sIndex!, (int)(++_sIndex));
                        _isDirty = true;
                    }
                }
            }, canExecute => HasSelectedItem && (_fIndex < FirstList.Count - 1 || _sIndex < SecondList.Count - 1));

            MoveUp = new RelayCommand(execute =>
            {
                if (_lastSelectedItem == false)
                {
                    if (_fIndex > 0)
                    {
                        FirstList.Move((int)_fIndex!, (int)(--_fIndex));
                        _isDirty = true;
                    }
                }
                else
                {
                    if (_sIndex > 0)
                    {
                        SecondList.Move((int)_sIndex!, (int)(--_sIndex));
                        _isDirty = true;
                    }
                }
            }, canExecute => HasSelectedItem && (_fIndex > 0 || _sIndex > 0));
        }

        public ObservableCollection<Item> FirstList { get; private set; } = null!;
        public ObservableCollection<Item> SecondList { get; private set; } = null!;

        private bool HasSelectedItem => FirstSelectedItem != null || SecondSelectedItem != null;

        private int? _fIndex;
        private Item? _fSelItem;
        public Item? FirstSelectedItem
        {
            get => _fSelItem;
            set
            {
                _lastSelectedItem = false;
                _fSelItem = value;
                _fIndex = value == null ? null : FirstList.IndexOf(value);
            }
        }

        private int? _sIndex;
        private Item? _sSelItem;
        public Item? SecondSelectedItem
        {
            get => _sSelItem;
            set
            {
                _lastSelectedItem = true;
                _sSelItem = value;
                _sIndex = value == null ? null : SecondList.IndexOf(value);
            }
        }

        public RelayCommand MoveToSecond { get; private init; }
        public RelayCommand MoveToFirst { get; private init; }

        public RelayCommand AddCommand { get; private init; }
        public RelayCommand DeleteCommand { get; private init; }
        public RelayCommand SaveCommand { get; private init; }

        public RelayCommand MoveUp { get; private init; }
        public RelayCommand MoveDown { get; private init; }

        private async Task InitializeLists()
        {
            if (File.Exists(_serializer.path))
            {
                string[] lists = (await _serializer.Deserialize()).Split(Environment.NewLine);

                FirstList = new ObservableCollection<Item>();

                foreach (string text in lists[0].Split('\t', StringSplitOptions.RemoveEmptyEntries))
                    FirstList.Add(new Item(text));

                SecondList = new ObservableCollection<Item>();

                foreach (string text in lists[1].Split('\t', StringSplitOptions.RemoveEmptyEntries))
                    SecondList.Add(new Item(text));
            }
            else
            {
                FirstList = new ObservableCollection<Item>() { new Item("Element_1") };
                SecondList = new ObservableCollection<Item>() { new Item("Element_2") };
            }

            _isDirty = false;
        }
    }
}