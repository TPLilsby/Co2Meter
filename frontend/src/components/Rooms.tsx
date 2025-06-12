import { useState, useEffect } from 'react';
import { roomApi } from '../services/api';
import type { Room } from '../types';
import type { RoomFilter } from '../services/api';
import './Rooms.css';

export default function Rooms() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [open, setOpen] = useState(false);
  const [editingRoom, setEditingRoom] = useState<Room | null>(null);
  const [newRoom, setNewRoom] = useState({
    name: '',
    description: '',
    floor: '',
    maxOccupancy: '',
  });
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [filter, setFilter] = useState<RoomFilter>({
    page: 1,
    pageSize: 10,
    sortBy: 'name',
    sortDescending: false,
  });

  useEffect(() => {
    loadRooms();
  }, [filter]);

  const loadRooms = async () => {
    try {
      const response = await roomApi.getRooms(filter);
      setRooms(response.data);
      setTotalCount(response.totalCount);
    } catch (error) {
      console.error('Error loading rooms:', error);
    }
  };

  const handleAddRoom = async () => {
    try {
      await roomApi.addRoom({
        name: newRoom.name,
        description: newRoom.description,
        floor: newRoom.floor,
        maxOccupancy: newRoom.maxOccupancy ? Number(newRoom.maxOccupancy) : undefined,
      });
      setOpen(false);
      setNewRoom({ name: '', description: '', floor: '', maxOccupancy: '' });
      loadRooms();
    } catch (error) {
      console.error('Error adding room:', error);
    }
  };

  const handleUpdateRoom = async () => {
    if (!editingRoom) return;
    try {
      await roomApi.updateRoom(editingRoom.id, {
        name: editingRoom.name,
        description: editingRoom.description,
        floor: editingRoom.floor,
        maxOccupancy: editingRoom.maxOccupancy,
      });
      setEditingRoom(null);
      loadRooms();
    } catch (error) {
      console.error('Error updating room:', error);
    }
  };

  const handleDeleteRoom = async (id: number) => {
    try {
      await roomApi.deleteRoom(id);
      loadRooms();
    } catch (error) {
      console.error('Error deleting room:', error);
    }
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
    setFilter(prev => ({ ...prev, page: newPage + 1 }));
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const newRowsPerPage = parseInt(event.target.value, 10);
    setRowsPerPage(newRowsPerPage);
    setPage(0);
    setFilter(prev => ({ ...prev, page: 1, pageSize: newRowsPerPage }));
  };

  return (
    <div className="rooms">
      <div className="header">
        <h2>Rooms</h2>
        <button className="add-button" onClick={() => setOpen(true)}>
          Add Room
        </button>
      </div>

      <div className="filters">
        <div className="filter-group">
          <label>Floor:</label>
          <input
            type="text"
            value={filter.floor || ''}
            onChange={(e) => setFilter(prev => ({ ...prev, floor: e.target.value }))}
          />
        </div>
        <div className="filter-group">
          <label>Min Occupancy:</label>
          <input
            type="number"
            value={filter.minOccupancy || ''}
            onChange={(e) => setFilter(prev => ({ ...prev, minOccupancy: Number(e.target.value) }))}
          />
        </div>
        <div className="filter-group">
          <label>Max Occupancy:</label>
          <input
            type="number"
            value={filter.maxOccupancy || ''}
            onChange={(e) => setFilter(prev => ({ ...prev, maxOccupancy: Number(e.target.value) }))}
          />
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Floor</th>
              <th>Max Occupancy</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {rooms.map((room) => (
              <tr key={room.id}>
                <td>{room.name}</td>
                <td>{room.description}</td>
                <td>{room.floor}</td>
                <td>{room.maxOccupancy}</td>
                <td>
                  <button
                    className="icon-button edit"
                    onClick={() => setEditingRoom(room)}
                    title="Edit"
                  >
                    ✏️
                  </button>
                  <button
                    className="icon-button delete"
                    onClick={() => handleDeleteRoom(room.id)}
                    title="Delete"
                  >
                    🗑️
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="pagination">
          <select
            value={rowsPerPage}
            onChange={handleChangeRowsPerPage}
          >
            <option value={5}>5 rows</option>
            <option value={10}>10 rows</option>
            <option value={25}>25 rows</option>
          </select>
          <div className="page-info">
            Page {page + 1} of {Math.ceil(totalCount / rowsPerPage)}
          </div>
          <button
            onClick={() => handleChangePage(null, page - 1)}
            disabled={page === 0}
          >
            Previous
          </button>
          <button
            onClick={() => handleChangePage(null, page + 1)}
            disabled={page >= Math.ceil(totalCount / rowsPerPage) - 1}
          >
            Next
          </button>
        </div>
      </div>

      {open && (
        <div className="modal">
          <div className="modal-content">
            <h3>Add New Room</h3>
            <div className="form-group">
              <label>Name:</label>
              <input
                type="text"
                value={newRoom.name}
                onChange={(e) => setNewRoom({ ...newRoom, name: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Description:</label>
              <input
                type="text"
                value={newRoom.description}
                onChange={(e) => setNewRoom({ ...newRoom, description: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Floor:</label>
              <input
                type="text"
                value={newRoom.floor}
                onChange={(e) => setNewRoom({ ...newRoom, floor: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Max Occupancy:</label>
              <input
                type="number"
                value={newRoom.maxOccupancy}
                onChange={(e) => setNewRoom({ ...newRoom, maxOccupancy: e.target.value })}
              />
            </div>
            <div className="modal-actions">
              <button onClick={() => setOpen(false)}>Cancel</button>
              <button onClick={handleAddRoom} className="primary">
                Add
              </button>
            </div>
          </div>
        </div>
      )}

      {editingRoom && (
        <div className="modal">
          <div className="modal-content">
            <h3>Edit Room</h3>
            <div className="form-group">
              <label>Name:</label>
              <input
                type="text"
                value={editingRoom.name}
                onChange={(e) => setEditingRoom({ ...editingRoom, name: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Description:</label>
              <input
                type="text"
                value={editingRoom.description}
                onChange={(e) => setEditingRoom({ ...editingRoom, description: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Floor:</label>
              <input
                type="text"
                value={editingRoom.floor}
                onChange={(e) => setEditingRoom({ ...editingRoom, floor: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Max Occupancy:</label>
              <input
                type="number"
                value={editingRoom.maxOccupancy || ''}
                onChange={(e) => setEditingRoom({ ...editingRoom, maxOccupancy: Number(e.target.value) })}
              />
            </div>
            <div className="modal-actions">
              <button onClick={() => setEditingRoom(null)}>Cancel</button>
              <button onClick={handleUpdateRoom} className="primary">
                Save
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
} 