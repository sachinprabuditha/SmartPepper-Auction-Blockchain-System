'use client';

import { useState, useEffect } from 'react';
import { useAccount, useWriteContract, useWaitForTransactionReceipt } from 'wagmi';
import { parseEther } from 'viem';
import { PEPPER_AUCTION_ABI, CONTRACT_ADDRESS } from '@/config/contracts';
import { auctionApi } from '@/lib/api';
import toast from 'react-hot-toast';
import { Loader2, TrendingUp, Zap } from 'lucide-react';

interface BidFormProps {
  auctionId: number;
  currentBid: string;
  minimumBid: string;
}

export function BidForm({ auctionId, currentBid, minimumBid }: BidFormProps) {
  const { address, isConnected } = useAccount();
  const [bidAmount, setBidAmount] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { writeContract, data: hash, isPending, error: writeError } = useWriteContract();
  const { isLoading: isConfirming, isSuccess } = useWaitForTransactionReceipt({ hash });

  const currentBidEth = parseFloat(currentBid || '0') / 1e18;
  const minBidEth = currentBidEth > 0 
    ? currentBidEth + 0.0001  // Min increment: 0.0001 ETH
    : parseFloat(minimumBid || '0') / 1e18;

  // Quick bid buttons
  const quickBids = [
    { label: 'Min', value: minBidEth },
    { label: '+0.001', value: minBidEth + 0.001 },
    { label: '+0.01', value: minBidEth + 0.01 },
    { label: '+0.1', value: minBidEth + 0.1 },
  ];

  const handleBidSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!isConnected || !address) {
      toast.error('Please connect your wallet');
      return;
    }

    const bid = parseFloat(bidAmount);
    if (isNaN(bid) || bid < minBidEth) {
      toast.error(`Bid must be at least ${minBidEth.toFixed(4)} ETH`);
      return;
    }

    try {
      setIsSubmitting(true);
      
      toast.loading('Submitting bid to blockchain...', { id: 'bid-tx' });

      // Write to smart contract
      writeContract({
        address: CONTRACT_ADDRESS as `0x${string}`,
        abi: PEPPER_AUCTION_ABI,
        functionName: 'placeBid',
        args: [BigInt(auctionId)],
        value: parseEther(bidAmount),
      });

    } catch (error: any) {
      console.error('Bid error:', error);
      toast.error(error.message || 'Failed to place bid', { id: 'bid-tx' });
      setIsSubmitting(false);
    }
  };

  // Handle transaction confirmation
  useEffect(() => {
    if (isSuccess && hash && address) {
      toast.success('Bid confirmed on blockchain!', { id: 'bid-tx' });
      
      // Record bid in backend
      auctionApi.placeBid(auctionId, {
        bidderAddress: address,
        amount: bidAmount,
        txHash: hash,
      }).then(() => {
        toast.success('Bid recorded successfully!');
        setBidAmount('');
        setIsSubmitting(false);
      }).catch((error) => {
        console.error('Failed to record bid:', error);
        toast.error('Bid placed but failed to record in database');
        setIsSubmitting(false);
      });
    }
  }, [isSuccess, hash, address, auctionId, bidAmount]);

  // Handle transaction error
  useEffect(() => {
    if (writeError) {
      toast.error('Transaction rejected', { id: 'bid-tx' });
      setIsSubmitting(false);
    }
  }, [writeError]);

  if (!isConnected) {
    return (
      <div className="text-center py-6 border-2 border-dashed border-gray-300 dark:border-gray-700 rounded-lg">
        <TrendingUp className="w-12 h-12 mx-auto mb-3 text-gray-400" />
        <p className="text-gray-500 dark:text-gray-400 mb-4">Connect your wallet to place a bid</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleBidSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium mb-2 dark:text-gray-300">
          Your Bid Amount
        </label>
        <div className="relative">
          <input
            type="number"
            step="0.0001"
            min={minBidEth}
            value={bidAmount}
            onChange={(e) => setBidAmount(e.target.value)}
            placeholder={`Minimum: ${minBidEth.toFixed(4)} ETH`}
            className="input pr-16"
          disabled={isSubmitting || isPending || isConfirming}
            required
          />
          <span className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-500 font-medium">
            ETH
          </span>
        </div>
        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
          Current bid: {currentBidEth.toFixed(4)} ETH • Minimum: {minBidEth.toFixed(4)} ETH
        </p>
      </div>

      {/* Quick bid buttons */}
      <div className="grid grid-cols-4 gap-2">
        {quickBids.map((quick) => (
          <button
            key={quick.label}
            type="button"
            onClick={() => setBidAmount(quick.value.toFixed(4))}
            className="px-3 py-2 text-sm bg-gray-100 hover:bg-gray-200 dark:bg-gray-700 dark:hover:bg-gray-600 rounded-lg transition-colors"
            disabled={isSubmitting || isPending || isConfirming}
          >
            {quick.label}
          </button>
        ))}
      </div>

      <button
        type="submit"
        className="btn-primary w-full flex items-center justify-center gap-2"
        disabled={isSubmitting || isPending || isConfirming || !bidAmount}
      >
        {(isPending || isConfirming) ? (
          <>
            <Loader2 className="w-4 h-4 animate-spin" />
            {isPending ? 'Confirm in wallet...' : 'Confirming on blockchain...'}
          </>
        ) : (
          <>
            <Zap className="w-4 h-4" />
            Place Bid
          </>
        )}
      </button>

      {isPending && (
        <p className="text-sm text-center text-yellow-600 dark:text-yellow-400">
          Please confirm the transaction in your wallet
        </p>
      )}

      {isConfirming && (
        <p className="text-sm text-center text-blue-600 dark:text-blue-400">
          <Loader2 className="w-4 h-4 animate-spin inline mr-2" />
          Waiting for blockchain confirmation...
        </p>
      )}

      {hash && (
        <p className="text-xs text-center">
          <a
            href={`https://sepolia.etherscan.io/tx/${hash}`}
            target="_blank"
            rel="noopener noreferrer"
            className="text-primary-600 hover:underline"
          >
            View transaction →
          </a>
        </p>
      )}
    </form>
  );
}
