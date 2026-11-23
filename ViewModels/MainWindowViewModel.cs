using System.Collections.ObjectModel;
using HubLink.Models;
using HubLink.Services;

namespace HubLink.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IShipmentService _service;
    private readonly AppSettings _settings;

    private List<Shipment> _allShipments = new();   // everything retrieved from the API
    private bool _hasLoaded;                        // true after at least one successful retrieval
    private DateTime _lastLoadedAt;

    private string _selectedStatus = "All";
    private bool _isLoading;
    private bool _isExporting;
    private string _statusMessage = "Ready.";
    private string _errorMessage = "";
    private string _oldDataMessage = "";
    private string _exportMessage = "";
    private bool _exportFailed;

    private int _totalCount, _pendingCount, _inTransitCount, _deliveredCount, _exceptionCount;

    public MainWindowViewModel(IShipmentService service, AppSettings settings)
    {
        _service = service;
        _settings = settings;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsLoading);
        ExportCommand = new AsyncRelayCommand(ExportAsync, () => !IsLoading && !_isExporting);
    }

    // ---------- Set by the View: opens the "Save file" dialog and returns the chosen path ----------
    public Func<string, Task<string?>>? PickSavePathAsync { get; set; }

    // ---------- Commands ----------
    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand ExportCommand { get; }

    // ---------- Data shown in the table (already filtered) ----------
    public ObservableCollection<Shipment> Shipments { get; } = new();

    public string[] StatusOptions { get; } = { "All", "Pending", "InTransit", "Delivered", "Exception" };

    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
                ApplyFilter();
        }
    }

    public string ServiceInfo => $"Branch: {_settings.BranchCode}   |   Service: {_settings.BaseUrl}";

    // ---------- Loading / messages ----------
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
                UpdateCommandStates();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }
    public bool HasError => _errorMessage != "";

    public string OldDataMessage
    {
        get => _oldDataMessage;
        private set
        {
            if (SetProperty(ref _oldDataMessage, value))
                OnPropertyChanged(nameof(IsShowingOldData));
        }
    }
    public bool IsShowingOldData => _oldDataMessage != "";

    public string ExportMessage
    {
        get => _exportMessage;
        private set => SetProperty(ref _exportMessage, value);
    }

    public bool ExportFailed
    {
        get => _exportFailed;
        private set => SetProperty(ref _exportFailed, value);
    }

    // ---------- Summary (always based on ALL retrieved shipments, not the filter) ----------
    public bool HasSummary => _hasLoaded;
    public string SummaryTitle => $"Summary of all retrieved shipments for {_settings.BranchCode}";
    public int TotalCount { get => _totalCount; private set => SetProperty(ref _totalCount, value); }
    public int PendingCount { get => _pendingCount; private set => SetProperty(ref _pendingCount, value); }
    public int InTransitCount { get => _inTransitCount; private set => SetProperty(ref _inTransitCount, value); }
    public int DeliveredCount { get => _deliveredCount; private set => SetProperty(ref _deliveredCount, value); }
    public int ExceptionCount { get => _exceptionCount; private set => SetProperty(ref _exceptionCount, value); }

    // ---------- Message shown when the table has no rows ----------
    public string EmptyMessage
    {
        get
        {
            if (!_hasLoaded)
                return HasError ? "No shipment data is available because retrieval failed." : "";

            if (_allShipments.Count == 0)
                return $"The service responded successfully, but there are no shipments for branch {_settings.BranchCode}.";

            if (Shipments.Count == 0)
                return $"No shipments match the status \"{SelectedStatus}\".";

            return "";
        }
    }
    public bool HasEmptyMessage => EmptyMessage != "";

    // ---------- Refresh ----------
    private async Task RefreshAsync()
    {
        if (IsLoading) return;   // prevent overlapping requests

        IsLoading = true;
        ErrorMessage = "";
        OldDataMessage = "";
        ExportMessage = "";
        StatusMessage = "Retrieving shipments...";

        try
        {
            var shipments = await _service.GetShipmentsAsync();

            _allShipments = shipments;
            _hasLoaded = true;
            _lastLoadedAt = DateTime.Now;

            ApplyFilter();
            UpdateSummary();

            StatusMessage = shipments.Count == 0
                ? $"Refreshed at {_lastLoadedAt:HH:mm:ss}. No shipments found for {_settings.BranchCode}."
                : $"Refreshed at {_lastLoadedAt:HH:mm:ss}. {shipments.Count} shipment(s) retrieved for {_settings.BranchCode}.";
        }
        catch (ShipmentServiceException ex)
        {
            HandleFailure(ex.Message);
        }
        catch (Exception ex)
        {
            HandleFailure("An unexpected error occurred: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
            RefreshEmptyMessage();
        }
    }

    private void HandleFailure(string message)
    {
        ErrorMessage = message;
        StatusMessage = "Refresh failed. Click Retry to try again.";

        // Old data may stay visible, but it must be clearly marked as old
        if (_hasLoaded)
        {
            OldDataMessage = $"The shipments below were loaded earlier at {_lastLoadedAt:HH:mm:ss} " +
                             "and may be out of date. The latest refresh did NOT succeed.";
        }
    }

    // ---------- Filter and summary ----------
    private void ApplyFilter()
    {
        Shipments.Clear();
        foreach (var s in _allShipments)
        {
            if (SelectedStatus == "All" || s.Status == SelectedStatus)
                Shipments.Add(s);
        }
        RefreshEmptyMessage();
    }

    private void UpdateSummary()
    {
        TotalCount = _allShipments.Count;
        PendingCount = _allShipments.Count(s => s.Status == "Pending");
        InTransitCount = _allShipments.Count(s => s.Status == "InTransit");
        DeliveredCount = _allShipments.Count(s => s.Status == "Delivered");
        ExceptionCount = _allShipments.Count(s => s.Status == "Exception");
        OnPropertyChanged(nameof(HasSummary));
    }

    private void RefreshEmptyMessage()
    {
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(HasEmptyMessage));
    }

    // ---------- CSV export (exports exactly what is displayed) ----------
    private async Task ExportAsync()
    {
        if (_isExporting) return;

        ExportFailed = false;
        ExportMessage = "";

        if (Shipments.Count == 0)
        {
            ExportFailed = true;
            ExportMessage = "There are no shipments to export. Nothing was saved.";
            return;
        }

        if (PickSavePathAsync == null) return;

        _isExporting = true;
        UpdateCommandStates();

        try
        {
            var toExport = Shipments.ToList();   // copy of the currently displayed records
            string fileName = $"shipments_{_settings.BranchCode}_{SelectedStatus}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            string? path = await PickSavePathAsync(fileName);
            if (path == null)
            {
                ExportMessage = "Export cancelled.";
                return;
            }

            await CsvExporter.ExportAsync(path, toExport);
            ExportMessage = $"Exported {toExport.Count} shipment(s) to: {path}";
        }
        catch (Exception ex)
        {
            ExportFailed = true;
            ExportMessage = "Export failed. The file was not saved correctly. " + ex.Message;
        }
        finally
        {
            _isExporting = false;
            UpdateCommandStates();
        }
    }

    private void UpdateCommandStates()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();
    }
}
