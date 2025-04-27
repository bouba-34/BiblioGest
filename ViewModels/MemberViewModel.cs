using BiblioGest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BiblioGest.Data;

namespace BiblioGest.ViewModels
{
    public class MemberViewModel : ViewModelBase
    {
        private readonly BiblioGestDbContext _context;
        private ObservableCollection<Member> _members;
        private Member _selectedMember;
        private Member _newMember;
        private string _searchText;
        private bool _isEditMode;
        private bool _isAddMode;
        private ObservableCollection<Loan> _memberLoans;

        public ObservableCollection<Member> Members
        {
            get => _members;
            set => SetProperty(ref _members, value);
        }

        public Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                SetProperty(ref _selectedMember, value);
                IsEditMode = value != null;

                if (value != null)
                {
                    // Create a copy for editing to avoid changing the original until save
                    NewMember = new Member
                    {
                        Id = value.Id,
                        FirstName = value.FirstName,
                        LastName = value.LastName,
                        Address = value.Address,
                        Email = value.Email,
                        Phone = value.Phone,
                        RegistrationDate = value.RegistrationDate,
                        IsActive = value.IsActive
                    };

                    // Load member's loans
                    LoadMemberLoans(value.Id);
                }
            }
        }

        public Member NewMember
        {
            get => _newMember;
            set => SetProperty(ref _newMember, value);
        }

        public ObservableCollection<Loan> MemberLoans
        {
            get => _memberLoans;
            set => SetProperty(ref _memberLoans, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                SearchMembers();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsAddMode
        {
            get => _isAddMode;
            set => SetProperty(ref _isAddMode, value);
        }

        public ICommand AddMemberCommand { get; private set; }
        public ICommand SaveMemberCommand { get; private set; }
        public ICommand DeleteMemberCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand ToggleStatusCommand { get; private set; }

        public MemberViewModel(BiblioGestDbContext context)
        {
            _context = context;

            // Initialize commands
            AddMemberCommand = new RelayCommand(AddMember);
            SaveMemberCommand = new RelayCommand(SaveMember, CanSaveMember);
            DeleteMemberCommand = new RelayCommand(DeleteMember, CanDeleteMember);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ToggleStatusCommand = new RelayCommand(ToggleStatus, CanToggleStatus);

            // Load data
            LoadMembers();

            // Initialize a new member
            NewMember = new Member { RegistrationDate = DateTime.Now, IsActive = true };
        }

        private void LoadMembers()
        {
            var membersList = _context.Members.ToList();
            Members = new ObservableCollection<Member>(membersList);
        }

        private void LoadMemberLoans(int memberId)
        {
            var loansList = _context.Loans
                .Include(l => l.Book)
                .Where(l => l.MemberId == memberId)
                .OrderByDescending(l => l.LoanDate)
                .ToList();

            MemberLoans = new ObservableCollection<Loan>(loansList);
        }

        private void SearchMembers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadMembers();
                return;
            }

            string searchLower = SearchText.ToLower();
            var filteredMembers = _context.Members
                .Where(m =>
                    m.FirstName.ToLower().Contains(searchLower) ||
                    m.LastName.ToLower().Contains(searchLower) ||
                    m.Email.ToLower().Contains(searchLower) ||
                    m.Phone.Contains(SearchText))
                .ToList();

            Members = new ObservableCollection<Member>(filteredMembers);
        }

        private void AddMember(object parameter)
        {
            NewMember = new Member { RegistrationDate = DateTime.Now, IsActive = true };
            IsAddMode = true;
            IsEditMode = false;
        }

        private bool CanSaveMember(object parameter)
        {
            return NewMember != null &&
                   !string.IsNullOrWhiteSpace(NewMember.FirstName) &&
                   !string.IsNullOrWhiteSpace(NewMember.LastName);
        }

        private void SaveMember(object parameter)
        {
            try
            {
                if (IsAddMode)
                {
                    _context.Members.Add(NewMember);
                }
                else if (IsEditMode)
                {
                    var memberToUpdate = _context.Members.Find(NewMember.Id);
                    if (memberToUpdate != null)
                    {
                        memberToUpdate.FirstName = NewMember.FirstName;
                        memberToUpdate.LastName = NewMember.LastName;
                        memberToUpdate.Address = NewMember.Address;
                        memberToUpdate.Email = NewMember.Email;
                        memberToUpdate.Phone = NewMember.Phone;
                        memberToUpdate.IsActive = NewMember.IsActive;
                    }
                }

                _context.SaveChanges();

                // Refresh member list
                LoadMembers();

                // Reset state
                IsAddMode = false;
                IsEditMode = false;
                NewMember = new Member { RegistrationDate = DateTime.Now, IsActive = true };

                MessageBox.Show("Member saved successfully!", "Success", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving member: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool CanDeleteMember(object parameter)
        {
            return SelectedMember != null;
        }

        private void DeleteMember(object parameter)
        {
            if (SelectedMember == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete member '{SelectedMember.FullName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Check if the member has loans
                    var hasLoans = _context.Loans.Any(l => l.MemberId == SelectedMember.Id);
                    if (hasLoans)
                    {
                        MessageBox.Show("Cannot delete this member because they have associated loans.",
                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _context.Members.Remove(SelectedMember);
                    _context.SaveChanges();

                    // Refresh member list
                    LoadMembers();

                    IsEditMode = false;

                    MessageBox.Show("Member deleted successfully!", "Success", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting member: {ex.Message}", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void CancelEdit(object parameter)
        {
            IsAddMode = false;
            IsEditMode = false;
            NewMember = new Member { RegistrationDate = DateTime.Now, IsActive = true };
        }

        private bool CanToggleStatus(object parameter)
        {
            return SelectedMember != null;
        }

        private void ToggleStatus(object parameter)
        {
            if (SelectedMember == null) return;

            try
            {
                var member = _context.Members.Find(SelectedMember.Id);
                if (member != null)
                {
                    member.IsActive = !member.IsActive;
                    _context.SaveChanges();

                    // Refresh member list
                    LoadMembers();

                    string status = member.IsActive ? "activated" : "deactivated";
                    MessageBox.Show($"Member {status} successfully!", "Success", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating member status: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}