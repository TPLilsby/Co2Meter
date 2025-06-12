import { useState, useEffect } from 'react';
import { co2Api, roomApi } from '../services/api';
import type { Co2Reading, Room } from '../types';
import type { Co2ReadingFilter } from '../services/api';
import './Co2Readings.css';

export default function Co2Readings() {
  const [readings, setReadings] = useState<Co2Reading[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);
  const [open, setOpen] = useState(false);
  const [newReading, setNewReading] = useState({
    roomId: '',
    ppm: '',
  });
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [filter, setFilter] = useState<Co2ReadingFilter>({
    page: 1,
    pageSize: 10,
    sortBy: 'timestamp',
    sortDescending: true,
  });

  useEffect(() => {
    loadData();
  }, [filter]);

  const loadData = async () => {
    try {
      const [readingsResponse, roomsResponse] = await Promise.all([
        co2Api.getReadings(filter),
        roomApi.getRooms(),
      ]);
      setReadings(readingsResponse.data);
      setTotalCount(readingsResponse.totalCount);
      setRooms(roomsResponse.data);
    } catch (error) {
      console.error('Error loading data:', error);
    }
  };

  const handleAddReading = async () => {
    try {
      await co2Api.addReading({
        roomId: Number(newReading.roomId),
        ppm: Number(newReading.ppm),
        timestamp: new Date().toISOString(),
      });
      setOpen(false);
      setNewReading({ roomId: '', ppm: '' });
      loadData();
    } catch (error) {
      console.error('Error adding reading:', error);
    }
  };

  const handlePageChange = (_: unknown, newPage: number) => {
    setPage(newPage);
    setFilter(prev => ({ ...prev, page: newPage + 1 }));
  };

  const handlePageSizeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newSize = parseInt(event.target.value, 10);
    setRowsPerPage(newSize);
    setPage(0);
    setFilter(prev => ({ ...prev, page: 1, pageSize: newSize }));
  };

  const handleRoomChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    setFilter(prev => ({ 
      ...prev, 
      roomId: value === '' ? undefined : Number(value)
    }));
  };

  return (
    <div className="co2-readings">
      <div className="header">
        <h2>CO2 Readings</h2>
        <button className="add-button" onClick={() => setOpen(true)}>
          Add Reading
        </button>
      </div>

      <div className="filters">
        <div className="filter-group">
          <label>Room:</label>
          <select
            value={filter.roomId || ''}
            onChange={(e) => {
              const value = e.target.value;
              setFilter(prev => ({ ...prev, roomId: value ? Number(value) : undefined }));
            }}
          >
            <option value="">All Rooms</option>
            {rooms.map((room) => (
              <option key={room.id} value={room.id}>
                {room.name}
              </option>
            ))}
          </select>
        </div>
        <div className="filter-group">
          <label>Min PPM:</label>
          <input
            type="number"
            value={filter.minPpm || ''}
            onChange={(e) => setFilter(prev => ({ ...prev, minPpm: Number(e.target.value) }))}
          />
        </div>
        <div className="filter-group">
          <label>Max PPM:</label>
          <input
            type="number"
            value={filter.maxPpm || ''}
            onChange={(e) => setFilter(prev => ({ ...prev, maxPpm: Number(e.target.value) }))}
          />
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Room</th>
              <th>PPM</th>
              <th>Timestamp</th>
            </tr>
          </thead>
          <tbody>
            {readings.map((reading) => (
              <tr key={reading.id}>
                <td>{reading.room?.name || 'Unknown'}</td>
                <td>{reading.ppm}</td>
                <td>{new Date(reading.timestamp).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="pagination">
          <select
            value={rowsPerPage}
            onChange={handlePageSizeChange}
          >
            <option value={5}>5 rows</option>
            <option value={10}>10 rows</option>
            <option value={25}>25 rows</option>
          </select>
          <div className="page-info">
            Page {page + 1} of {Math.ceil(totalCount / rowsPerPage)}
          </div>
          <button
            onClick={() => handlePageChange(null, page - 1)}
            disabled={page === 0}
          >
            Previous
          </button>
          <button
            onClick={() => handlePageChange(null, page + 1)}
            disabled={page >= Math.ceil(totalCount / rowsPerPage) - 1}
          >
            Next
          </button>
        </div>
      </div>

      {open && (
        <div className="modal">
          <div className="modal-content">
            <h3>Add New CO2 Reading</h3>
            <div className="form-group">
              <label>Room:</label>
              <select
                value={newReading.roomId}
                onChange={(e) => setNewReading({ ...newReading, roomId: e.target.value })}
              >
                {rooms.map((room) => (
                  <option key={room.id} value={room.id}>
                    {room.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>PPM:</label>
              <input
                type="number"
                value={newReading.ppm}
                onChange={(e) => setNewReading({ ...newReading, ppm: e.target.value })}
              />
            </div>
            <div className="modal-actions">
              <button onClick={() => setOpen(false)}>Cancel</button>
              <button onClick={handleAddReading} className="primary">
                Add
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
} 